using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace KUtilitiesCore.Tests.Data
{
    [TestClass]
    public class DataErrorInfoExtTests
    {
        private class TestDataErrorInfo : IDataErrorInfo
        {
            public string Error { get; set; }
            public string this[string columnName]
            {
                get
                {   
                    return DataErrorInfoExt.GetErrorText(this, columnName); 
                }
            }
            [Required(AllowEmptyStrings = false)]
            public string Name { get; set; }
            [Range(0,120)]
            public int Age { get; set; }
        }

        [TestMethod]
        public void GetErrorText_ShouldReturnErrorForNonNestedProperty()
        {
            // Arrange
            var testObject = new TestDataErrorInfo { Name = "", Age = 25 };

            // Act
            var errorText = testObject[nameof(TestDataErrorInfo.Name)];

            // Assert
            Assert.IsTrue(errorText.Length>0);
        }

        [TestMethod]
        public void GetErrorText_ShouldReturnEmptyForValidNonNestedProperty()
        {
            // Arrange
            var testObject = new TestDataErrorInfo { Name = "John", Age = 25 };

            // Act
            var errorText = testObject[nameof(TestDataErrorInfo.Name)];

            // Assert
            Assert.AreEqual(string.Empty, errorText);
        }

        [TestMethod]
        public void GetErrorText_ShouldReturnErrorForInvalidNestedProperty()
        {
            // Arrange
            var testObject = new TestDataErrorInfo { Name = "John", Age = 130 };

            // Act
            var errorText = testObject[nameof(TestDataErrorInfo.Age)];

            // Assert
            Assert.IsTrue(errorText.Length>0);
        }

        [TestMethod]
        public void GetErrorText_ShouldReturnEmptyForNonExistentProperty()
        {
            // Arrange
            var testObject = new TestDataErrorInfo { Name = "John", Age = 25 };

            // Act
            var errorText = DataErrorInfoExt.GetErrorText(testObject, "NonExistentProperty");

            // Assert
            Assert.AreEqual(string.Empty, errorText);
        }
    }
}

