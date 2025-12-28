using BiochemSimulator.ViewModels;
using System;
using Xunit;

namespace BiochemSimulator.Tests.ViewModels
{
    public class RelayCommandTests
    {
        #region RelayCommand Tests

        [Fact]
        public void RelayCommand_Execute_ShouldInvokeAction()
        {
            // Arrange
            bool wasExecuted = false;
            var command = new RelayCommand(() => wasExecuted = true);

            // Act
            command.Execute(null);

            // Assert
            Assert.True(wasExecuted);
        }

        [Fact]
        public void RelayCommand_Execute_ShouldPassParameter()
        {
            // Arrange
            object? receivedParameter = null;
            var command = new RelayCommand(param => receivedParameter = param);

            // Act
            command.Execute("TestParameter");

            // Assert
            Assert.Equal("TestParameter", receivedParameter);
        }

        [Fact]
        public void RelayCommand_CanExecute_ShouldReturnTrue_WhenNoPredicateProvided()
        {
            // Arrange
            var command = new RelayCommand(() => { });

            // Act
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void RelayCommand_CanExecute_ShouldReturnPredicateResult()
        {
            // Arrange
            var command = new RelayCommand(() => { }, () => false);

            // Act
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.False(canExecute);
        }

        [Fact]
        public void RelayCommand_CanExecute_ShouldEvaluateParameterPredicate()
        {
            // Arrange
            var command = new RelayCommand(
                _ => { },
                param => param != null && (int)param > 10);

            // Act & Assert
            Assert.False(command.CanExecute(5));
            Assert.True(command.CanExecute(15));
        }

        [Fact]
        public void RelayCommand_Constructor_ShouldThrowOnNullExecute()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RelayCommand((Action)null!));
        }

        #endregion

        #region RelayCommand<T> Tests

        [Fact]
        public void GenericRelayCommand_Execute_ShouldInvokeActionWithTypedParameter()
        {
            // Arrange
            string? receivedValue = null;
            var command = new RelayCommand<string>(value => receivedValue = value);

            // Act
            command.Execute("TestValue");

            // Assert
            Assert.Equal("TestValue", receivedValue);
        }

        [Fact]
        public void GenericRelayCommand_Execute_ShouldHandleNullParameter()
        {
            // Arrange
            string? receivedValue = "initial";
            var command = new RelayCommand<string>(value => receivedValue = value);

            // Act
            command.Execute(null);

            // Assert
            Assert.Null(receivedValue);
        }

        [Fact]
        public void GenericRelayCommand_CanExecute_ShouldReturnTrue_WhenNoPredicateProvided()
        {
            // Arrange
            var command = new RelayCommand<string>(_ => { });

            // Act
            bool canExecute = command.CanExecute("test");

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void GenericRelayCommand_CanExecute_ShouldEvaluatePredicate()
        {
            // Arrange
            var command = new RelayCommand<int>(
                _ => { },
                value => value > 0);

            // Act & Assert
            Assert.False(command.CanExecute(-5));
            Assert.True(command.CanExecute(5));
        }

        [Fact]
        public void GenericRelayCommand_CanExecute_ShouldHandleNullForValueType()
        {
            // Arrange
            var command = new RelayCommand<int>(
                _ => { },
                value => value > 0);

            // Act - null parameter with value type should return true if no predicate issues
            bool canExecute = command.CanExecute(null);

            // Assert - When parameter is null and type is value type, returns true if canExecute is null
            Assert.True(canExecute);
        }

        [Fact]
        public void GenericRelayCommand_Constructor_ShouldThrowOnNullExecute()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RelayCommand<string>(null!));
        }

        #endregion

        #region AsyncRelayCommand Tests

        [Fact]
        public async System.Threading.Tasks.Task AsyncRelayCommand_Execute_ShouldInvokeAsyncAction()
        {
            // Arrange
            bool wasExecuted = false;
            var command = new AsyncRelayCommand(async () =>
            {
                await System.Threading.Tasks.Task.Delay(10);
                wasExecuted = true;
            });

            // Act
            command.Execute(null);
            await System.Threading.Tasks.Task.Delay(50); // Wait for async operation

            // Assert
            Assert.True(wasExecuted);
        }

        [Fact]
        public void AsyncRelayCommand_CanExecute_ShouldReturnTrue_WhenNotExecuting()
        {
            // Arrange
            var command = new AsyncRelayCommand(async () => await System.Threading.Tasks.Task.Delay(10));

            // Act
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.True(canExecute);
        }

        [Fact]
        public void AsyncRelayCommand_CanExecute_ShouldReturnFalse_WhenPredicateReturnsFalse()
        {
            // Arrange
            var command = new AsyncRelayCommand(
                async () => await System.Threading.Tasks.Task.Delay(10),
                () => false);

            // Act
            bool canExecute = command.CanExecute(null);

            // Assert
            Assert.False(canExecute);
        }

        [Fact]
        public void AsyncRelayCommand_Constructor_ShouldThrowOnNullExecute()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AsyncRelayCommand((Func<System.Threading.Tasks.Task>)null!));
        }

        #endregion
    }
}
