﻿namespace Core.Entities
{
    public class FileEntity
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public Stream FileContent { get; set; }
    }
}
