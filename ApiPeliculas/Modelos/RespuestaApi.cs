using System.Net;

namespace ApiPeliculas.Modelos
{
    public class RespuestaApi
    {
        public RespuestaApi()
        {
            ErrorMessages = new List<String>();
        }

        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }
        public object Result { get; set; }
    }
}
