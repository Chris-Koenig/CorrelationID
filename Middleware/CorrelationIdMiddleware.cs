using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace CorrelationID.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate  _next;
        private static string CorrelationHeaderPropName = "x-correlation-id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Trace.CorrelationManager.ActivityId = GetOrCreateCorrelationId(context);

            // apply the correlation ID to the response header for client side handling
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Add(CorrelationHeaderPropName, new[] { Trace.CorrelationManager.ActivityId.ToString() });
                                               
                return Task.CompletedTask;
            });

            await _next(context);
        }

        
        private static Guid GetOrCreateCorrelationId(HttpContext context)
        {
            var header = context.Request.Headers[CorrelationHeaderPropName];
            
            var correlationId = Guid.NewGuid();

            if (header.Count > 0)
            {
                if (Guid.TryParse(header[0], out var correlationIdFromHeader) && correlationIdFromHeader != Guid.Empty)
                {
                    correlationId = correlationIdFromHeader;
                }
            }
           
            return correlationId;
           
        }
    }
}