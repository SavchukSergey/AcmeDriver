using System;

namespace AcmeDriver {
    public class CLIException : Exception {

        public CLIException(string message) : base(message) {
        }

        public CLIException(string message, Exception innerException) : base(message, innerException) {
        }

    }
}