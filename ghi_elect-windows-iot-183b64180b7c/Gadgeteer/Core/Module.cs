using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GHIElectronics.UAP.Gadgeteer {
	public abstract class Module {
		private Dictionary<int, ISocket> providedSockets;

		public abstract string Name { get; }
		public abstract string Manufacturer { get; }

		public virtual int RequiredSockets => 1;
		public int ProvidedSockets => this.providedSockets.Count;

		protected Module() {
			this.providedSockets = new Dictionary<int, ISocket>();
		}

		protected virtual Task Initialize() {
			throw new InvalidModuleDefinitionException($"This module does not overload the proper {nameof(this.Initialize)} method.");
		}

		protected virtual Task Initialize(ISocket parentSocket) {
			throw new InvalidModuleDefinitionException($"This module does not overload the proper {nameof(this.Initialize)} method.");
		}

		protected virtual Task Initialize(params ISocket[] parentSockets) {
			throw new InvalidModuleDefinitionException($"This module does not overload the proper {nameof(this.Initialize)} method.");
		}

		protected Socket CreateSocket(int socketNumber) {
			var socket = new Socket(socketNumber);

			this.providedSockets.Add(socket.Number, socket);

			return socket;
		}

        public virtual void SetDebugLed(bool state) {
            throw new NotSupportedException();
        }

		public ISocket GetProvidedSocket(int socketNumber) {
			if (!this.providedSockets.ContainsKey(socketNumber))
				throw new ArgumentException("That socket does not exist.", nameof(socketNumber));

			return this.providedSockets[socketNumber];
		}

		public static async Task<T> CreateAsync<T>(params ISocket[] parentSockets) where T : Module, new() {
			var module = new T();

			if (module.RequiredSockets != parentSockets.Length)
				throw new ArgumentException($"Invalid number of sockets passed. Expected {module.RequiredSockets}.", nameof(parentSockets));

			if (module.RequiredSockets == 0) {
				await module.Initialize();
			}
			else if (module.RequiredSockets == 1) {
				await module.Initialize(parentSockets[0]);
			}
			else {
				await module.Initialize(parentSockets);
			}

			return module;
		}
	}
}