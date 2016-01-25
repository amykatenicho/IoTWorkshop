using System;

namespace GHIElectronics.UAP.Gadgeteer {
	public class SocketInterfaceCreationException : Exception {
		public SocketInterfaceCreationException() { }
		public SocketInterfaceCreationException(string message) : base(message) { }
		public SocketInterfaceCreationException(string message, Exception inner) : base(message, inner) { }
	}

	public class UnsupportedSocketTypeException : Exception {
		public UnsupportedSocketTypeException() { }
		public UnsupportedSocketTypeException(string message) : base(message) { }
		public UnsupportedSocketTypeException(string message, Exception inner) : base(message, inner) { }
    }

    public class UnsupportedPinModeException : Exception {
        public UnsupportedPinModeException() { }
        public UnsupportedPinModeException(string message) : base(message) { }
        public UnsupportedPinModeException(string message, Exception inner) : base(message, inner) { }
    }

    public class InvalidModuleDefinitionException : Exception {
		public InvalidModuleDefinitionException() { }
		public InvalidModuleDefinitionException(string message) : base(message) { }
		public InvalidModuleDefinitionException(string message, Exception inner) : base(message, inner) { }
	}
}