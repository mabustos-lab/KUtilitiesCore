﻿using System;
using System.Threading;

namespace KUtilitiesCore.Logger.Helpers
{
    internal sealed class LoggerScope : IDisposable
    {
        private static readonly AsyncLocal<object?> _currentScope = new();

        public object? State { get; }

        public LoggerScope(object state)
        {
            State = state;
            _currentScope.Value = this;
        }

        public static LoggerScope? Current => _currentScope.Value as LoggerScope;

        public void Dispose()
        {
            _currentScope.Value = null;
        }
    }
}
