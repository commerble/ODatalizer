﻿using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.UriParser;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Deltas;
using ODatalizer.EFCore.Routing;
using System.Collections.Generic;
using ODatalizer.EFCore.Converters;

namespace ODatalizer.EFCore
{
    public abstract class ODatalizerController<TDbContext> : ODataController where TDbContext : DbContext
    {
        protected readonly TDbContext DbContext;
        private readonly ODatalizerVisitor _visitor;
        private readonly IOptions<MvcOptions> _mvcOptions;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ILogger<ODatalizerController<TDbContext>> _logger;
        private readonly IAuthorizationService _authorization;
        private readonly bool _authorize;
        public ODatalizerController(IServiceProvider sp, bool authorize = false)
        {
            DbContext = sp.GetRequiredService<TDbContext>();
            _mvcOptions = sp.GetRequiredService<IOptions<MvcOptions>>();
            _modelMetadataProvider = sp.GetRequiredService<IModelMetadataProvider>();
            _logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<ODatalizerController<TDbContext>>();
            _authorization = sp.GetService<IAuthorizationService>();
            _authorize = authorize;
            _visitor = new ODatalizerVisitor(DbContext, sp.GetService<IEnumerable<ITypeConverter>>());
        }

        [EnableQuery(PageSize = ODatalizerEndpoint.DefaultPageSize)]
        public virtual async Task<IActionResult> Get()
        {
            var odataPath = Request.ODataFeature().Path;

            if (odataPath == null)
                return NotFound();

            if (odataPath.Any(segment => segment is UnrecognizedPathSegment))
                return BadRequest("Invalid URI Path");

            try
            {
                await _visitor.VisitAsync(odataPath);
            }
            catch (NullReferenceException nrex)
            {
                _logger.LogInformation(ODatalizerLogEvents.DynamicVisitorNullReferenced, nrex, "Null on {@Path} at {@Index}", Request.Path, _visitor.Index);
                return NotFound();
            }
            catch (NotSupportedException)
            {
                return StatusCode(501);
            }
            catch (Exception ex) {
                _logger.LogError(ex, ex.Message);
                throw;
            }

            if (_visitor.BadRequest)
                return BadRequest(ModelState);

            if (_visitor.NotFound || _visitor.Result == null)
                return NotFound();

            if (_authorize)
            {
                var authorizationResult = await _authorization.AuthorizeAsync(User, _visitor.AuthorizationInfo, "Read");

                if (!authorizationResult.Succeeded)
                {
                    if (User.Identity.IsAuthenticated)
                        return Forbid();
                    else
                        return Unauthorized();
                }
            }

            return Ok(_visitor.Result);
        }

        public virtual async Task<IActionResult> Post()
        {
            var odataPath = Request.ODataFeature().Path;

            if (odataPath == null)
                return NotFound();

            if (odataPath.Any(segment => segment is UnrecognizedPathSegment))
                return BadRequest("Invalid URI Path");

            try
            {
                await _visitor.VisitAsync(odataPath);
            }
            catch (NullReferenceException nrex)
            {
                _logger.LogInformation(ODatalizerLogEvents.DynamicVisitorNullReferenced, nrex, "Null on {@Path} at {@Index}", Request.Path, _visitor.Index);
                return NotFound();
            }
            catch (NotSupportedException)
            {
                return StatusCode(501);
            }

            if (_visitor.BadRequest)
                return BadRequest(ModelState);

            if (_visitor.NotFound)
                return NotFound();

            if (_authorize)
            {
                var authorizationResult = await _authorization.AuthorizeAsync(User, _visitor.AuthorizationInfo, "Write");

                if (!authorizationResult.Succeeded)
                {
                    if (User.Identity.IsAuthenticated)
                        return Forbid();
                    else
                        return Unauthorized();
                }
            }

            var result = await FormatReadAsync(_visitor.ResultType);

            if (result == null || result.HasError || result.IsModelSet == false || result.Model == null || TryValidateModel(result.Model) == false)
                return BadRequest(this.CreateSerializableErrorFromModelState());

            if (_visitor.PropertySetter != null)
            {
                _visitor.PropertySetter.Invoke(result.Model);
            }
            else
            {
                _visitor.Result.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance).Invoke(_visitor.Result, new[] { result.Model });
            }

            await DbContext.SaveChangesAsync();

            return Created(result.Model);
        }

        public virtual async Task<IActionResult> Put()
        {
            var odataPath = Request.ODataFeature().Path;

            if (odataPath == null)
                return NotFound();

            if (odataPath.Any(segment => segment is UnrecognizedPathSegment))
                return BadRequest("Invalid URI Path");

            try
            {
                await _visitor.VisitAsync(odataPath);
            }
            catch (NullReferenceException nrex)
            {
                _logger.LogInformation(ODatalizerLogEvents.DynamicVisitorNullReferenced, nrex, "Null on {@Path} at {@Index} of", Request.Path, _visitor.Index);
                return NotFound();
            }
            catch (NotSupportedException)
            {
                return StatusCode(501);
            }

            if (_visitor.BadRequest)
                return BadRequest(ModelState);

            if (_visitor.NotFound || _visitor.Result == null)
                return NotFound();

            if (_authorize)
            {
                var authorizationResult = await _authorization.AuthorizeAsync(User, _visitor.AuthorizationInfo, "Write");

                if (!authorizationResult.Succeeded)
                {
                    if (User.Identity.IsAuthenticated)
                        return Forbid();
                    else
                        return Unauthorized();
                }
            }

            var result = await FormatReadAsync(_visitor.ResultType);

            if (result == null || result.HasError || result.IsModelSet == false || result.Model == null || TryValidateModel(result.Model) == false)
                return BadRequest(this.CreateSerializableErrorFromModelState());


            if (odataPath.LastSegment is PropertySegment || odataPath.LastSegment is ValueSegment)
            {
                _visitor.PropertySetter.Invoke(result.Model);
            }
            else
            {
                DbContext.Entry(_visitor.Result).State = EntityState.Detached;
                DbContext.Entry(result.Model).State = EntityState.Modified;
            }

            await DbContext.SaveChangesAsync();

            return NoContent();
        }

        public virtual async Task<IActionResult> Patch()
        {
            var odataPath = Request.ODataFeature().Path;

            if (odataPath == null)
                return NotFound();

            if (odataPath.Any(segment => segment is UnrecognizedPathSegment))
                return BadRequest("Invalid URI Path");

            try
            {
                await _visitor.VisitAsync(odataPath);
            }
            catch (NullReferenceException nrex)
            {
                _logger.LogInformation(ODatalizerLogEvents.DynamicVisitorNullReferenced, nrex, "Null on {@Path} at {@Index}", Request.Path, _visitor.Index);
                return NotFound();
            }
            catch (NotSupportedException)
            {
                return StatusCode(501);
            }

            if (_visitor.BadRequest)
                return BadRequest(ModelState);

            if (_visitor.NotFound || _visitor.Result == null)
                return NotFound();

            if (_authorize)
            {
                var authorizationResult = await _authorization.AuthorizeAsync(User, _visitor.AuthorizationInfo, "Write");

                if (!authorizationResult.Succeeded)
                {
                    if (User.Identity.IsAuthenticated)
                        return Forbid();
                    else
                        return Unauthorized();
                }
            }

            var type = typeof(Delta<>).MakeGenericType(_visitor.ResultType);
            var result = await FormatReadAsync(type);

            if (result == null || result.HasError || result.IsModelSet == false || result.Model == null || TryValidateModel(result.Model) == false)
                return BadRequest(this.CreateSerializableErrorFromModelState());

            var method = type.GetMethod("Patch", BindingFlags.Public | BindingFlags.Instance, new[] { _visitor.ResultType });

            method.Invoke(result.Model, new[] { _visitor.Result });

            await DbContext.SaveChangesAsync();

            return NoContent();
        }

        public virtual async Task<IActionResult> Delete()
        {
            var odataPath = Request.ODataFeature().Path;

            if (odataPath == null)
                return NotFound();

            if (odataPath.Any(segment => segment is UnrecognizedPathSegment))
                return BadRequest("Invalid URI Path");

            try
            {
                await _visitor.VisitAsync(odataPath);
            }
            catch (NullReferenceException nrex)
            {
                _logger.LogInformation(ODatalizerLogEvents.DynamicVisitorNullReferenced, nrex, "Null on {@Path} at {@Index}", Request.Path, _visitor.Index);
                return NotFound();
            }
            catch (NotSupportedException)
            {
                return StatusCode(501);
            }

            if (_visitor.BadRequest)
                return BadRequest(ModelState);

            if (_visitor.NotFound || _visitor.Result == null)
                return NotFound();

            if (_authorize)
            {
                var authorizationResult = await _authorization.AuthorizeAsync(User, _visitor.AuthorizationInfo, "Write");

                if (!authorizationResult.Succeeded)
                {
                    if (User.Identity.IsAuthenticated)
                        return Forbid();
                    else
                        return Unauthorized();
                }
            }

            if (_visitor.PropertySetter != null)
            {
                _visitor.PropertySetter.Invoke(null);
            }
            else
            {
                DbContext.Remove(_visitor.Result);
            }

            await DbContext.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<InputFormatterResult> FormatReadAsync(Type type)
        {
            var context = new InputFormatterContext(
                    HttpContext,
                    "model",
                    ModelState,
                    _modelMetadataProvider.GetMetadataForType(type ?? typeof(object)),
                    (stream, encoding) => new StreamReader(stream, encoding));

            var formatter = _mvcOptions.Value.InputFormatters.FirstOrDefault(f => f.CanRead(context));

            return await formatter?.ReadAsync(context);
        }
    }
}
