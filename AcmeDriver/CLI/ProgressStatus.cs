using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeDriver.CLI {
	public class ProgressStatus : IDisposable {

		private readonly SemaphoreSlim _dirty = new SemaphoreSlim(1, int.MaxValue);
		private readonly CancellationTokenSource _done = new CancellationTokenSource();
		private readonly IList<ProgressStatusItem?> _lines = new List<ProgressStatusItem?>();

		public Task RunAsync<T>(IEnumerable<T> tasks, Func<T, ProgressStatusItem, IAsyncEnumerable<ProgressStatusItem>> selector) {
			return RunAsync(tasks.Select(task => {
				Func<ProgressStatusItem, IAsyncEnumerable<ProgressStatusItem>> source = item => selector(task, item);
				return source;
			}));
		}

		public async Task RunAsync(IEnumerable<Func<ProgressStatusItem, IAsyncEnumerable<ProgressStatusItem>>> tasks) {
			var primaryTasks = Task.WhenAll(tasks.Select(async (task, index) => {
				var item = new ProgressStatusItem();
				_lines.Add(item);
				await foreach (var _ in task(item)) {
					_dirty.Release();
				}
			}));
			var updateTask = UpdateConsoleAsync();
			await primaryTasks;
			_done.Cancel();
			await updateTask;
		}

		public void Dispose() {
			_dirty.Dispose();
		}

		private async Task UpdateConsoleAsync() {
			var position = Console.CursorTop;
			while (!_done.IsCancellationRequested) {
				await _dirty.WaitAsync(_done.Token);
				Console.SetCursorPosition(0, position);
				foreach (var line in _lines) {
					if (line != null) {
						Console.Write($"{line.Subject}: {line.Info} ");
						var backup = Console.ForegroundColor;
						Console.ForegroundColor = line.Color ?? Console.ForegroundColor;
						Console.Write(line.Status);
						Console.ForegroundColor = backup;
					}
					Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
					Console.WriteLine();
				}
				Console.WriteLine();
			}

		}
	}

	public class ProgressStatusItem {

		public string? Subject { get; set; }

		public string? Info { get; set; }

		public string? Status { get; set; }

		public ConsoleColor? Color { get; set; }

		public ProgressStatusItem SetStatus(string status) {
			Status = status;
			return this;
		}

		public ProgressStatusItem SetSubject(string subject) {
			Subject = subject;
			return this;
		}

		public ProgressStatusItem SetInfo(string? info) {
			Info = info;
			return this;
		}

		public ProgressStatusItem SetColor(ConsoleColor? color) {
			Color = color;
			return this;
		}

		public ProgressStatusItem SetOk() {
			Status = "ok";
			Color = ConsoleColor.Green;
			Info = null;
			return this;
		}

		public ProgressStatusItem SetFailed() {
			Status = "failed";
			Color = ConsoleColor.Red;
			Info = null;
			return this;
		}

		public ProgressStatusItem SetPending() {
			Status = "pending";
			Color = ConsoleColor.Yellow;
			return this;
		}

		public ProgressStatusItem SetSkipped() {
			Status = "skipped";
			Color = null;
			Info = null;
			return this;
		}

	}
}