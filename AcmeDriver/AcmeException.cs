using System;

namespace AcmeDriver {
    public class AcmeException : Exception {

        public string Type { get; }

        public int Status { get; }

        public AcmeException(AcmeExceptionInfo info) : base(info.Detail) {
            Type = info.Type;
            Status = info.Status;
        }

    }
}
