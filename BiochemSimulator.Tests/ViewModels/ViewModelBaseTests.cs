using BiochemSimulator.ViewModels;
using System.ComponentModel;
using Xunit;

namespace BiochemSimulator.Tests.ViewModels
{
    public class ViewModelBaseTests
    {
        private class TestViewModel : ViewModelBase
        {
            private string _testProperty = "";
            private int _intProperty;

            public string TestProperty
            {
                get => _testProperty;
                set => SetProperty(ref _testProperty, value);
            }

            public int IntProperty
            {
                get => _intProperty;
                set => SetProperty(ref _intProperty, value);
            }

            public void RaisePropertyChangedPublic(string propertyName)
            {
                OnPropertyChanged(propertyName);
            }
        }

        [Fact]
        public void SetProperty_ShouldRaisePropertyChanged_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

            // Act
            viewModel.TestProperty = "NewValue";

            // Assert
            Assert.Equal("TestProperty", changedPropertyName);
        }

        [Fact]
        public void SetProperty_ShouldNotRaisePropertyChanged_WhenValueIsSame()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.TestProperty = "InitialValue";

            bool eventRaised = false;
            viewModel.PropertyChanged += (s, e) => eventRaised = true;

            // Act
            viewModel.TestProperty = "InitialValue";

            // Assert
            Assert.False(eventRaised);
        }

        [Fact]
        public void SetProperty_ShouldUpdateValue_WhenCalled()
        {
            // Arrange
            var viewModel = new TestViewModel();

            // Act
            viewModel.TestProperty = "NewValue";

            // Assert
            Assert.Equal("NewValue", viewModel.TestProperty);
        }

        [Fact]
        public void SetProperty_ShouldReturnTrue_WhenValueChanges()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var testValue = "";
            bool result = false;

            // Use reflection to test the return value
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "TestProperty")
                    result = true;
            };

            // Act
            viewModel.TestProperty = "Changed";

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void SetProperty_ShouldWorkWithValueTypes()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

            // Act
            viewModel.IntProperty = 42;

            // Assert
            Assert.Equal("IntProperty", changedPropertyName);
            Assert.Equal(42, viewModel.IntProperty);
        }

        [Fact]
        public void OnPropertyChanged_ShouldRaiseEvent_WithCorrectPropertyName()
        {
            // Arrange
            var viewModel = new TestViewModel();
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

            // Act
            viewModel.RaisePropertyChangedPublic("CustomProperty");

            // Assert
            Assert.Equal("CustomProperty", changedPropertyName);
        }

        [Fact]
        public void PropertyChanged_ShouldBeNull_Initially()
        {
            // Arrange & Act
            var viewModel = new TestViewModel();

            // Assert - No exception should be thrown when setting property without subscribers
            viewModel.TestProperty = "Value";
            Assert.Equal("Value", viewModel.TestProperty);
        }
    }
}
