namespace DemoProject_backend.Dtos
{
    public class PdfCompressorDto
    {
        public class StartTaskResponse
        {
            public string task { get; set; }
            public string server { get; set; }
        }

        public class UploadedFileResponse
        {
            public string server_filename { get; set; }
        }

    }
}
