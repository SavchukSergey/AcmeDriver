namespace AcmeDriver.JWK {
    public abstract class PrivateJsonWebKey : JsonWebKey {

        public abstract PublicJsonWebKey GetPublicJwk();

        public abstract byte[] SignData(byte[] data);

    }
}
