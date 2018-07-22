using System;

namespace SharedLibrary
{
    public class ApplicationNotFoundException : Exception
    {
        public ApplicationNotFoundException()
        {
        }

        public ApplicationNotFoundException(string message) : base(message)
        {
        }

        public ApplicationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public class DatasetNotFoundException : Exception
    {
        public DatasetNotFoundException()
        {
        }

        public DatasetNotFoundException(string message) : base(message)
        {
        }

        public DatasetNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}