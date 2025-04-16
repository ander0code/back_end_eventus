using System;
using System.Collections.Generic;

namespace back_end.Core.Helpers
{
    // Clase para estandarizar las respuestas de la API
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Constructor para respuestas exitosas
        public ApiResponse(T data, string message )
        {
            Success = true;
            Message = message ?? "Operación completada con éxito";
            Data = data;
            Errors = Array.Empty<string>(); // Inicializar Errors para evitar error de non-null
        }

        // Constructor para respuestas de error
        public ApiResponse(string errorMessage, IEnumerable<string> errors )
        {
            Success = false;
            Message = errorMessage;
            Errors = errors ?? Array.Empty<string>();
        }
    }

    // Métodos de extensión para facilitar el uso en controladores
    public static class ResponseExtensions
    {
        public static ApiResponse<T> Success<T>(this T data, string message )
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<object> Error(this object _, string message, IEnumerable<string> errors )
        {
            return new ApiResponse<object>(message, errors);
        }
    }
}