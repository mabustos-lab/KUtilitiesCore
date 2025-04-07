using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCoreTests.Tracking
{
    [TestClass()]
    public class ChangeTrackingBaseTests
    {
        #region Methods

        [TestMethod()]
        public void BeginEditTest()
        {
            var obj = new TestChangeTracking();
            obj.TestProperty = "Initial Value";
            obj.BeginEdit();
            obj.TestProperty = "Changed Value";

            Assert.AreEqual("Changed Value", obj.TestProperty);
            Assert.IsTrue(obj.IsChanged);
        }

        [TestMethod()]
        public void CancelEditTest()
        {
            var obj = new TestChangeTracking();
            obj.TestProperty = "Initial Value";
            obj.BeginEdit();
            obj.TestProperty = "Changed Value";
            obj.CancelEdit();

            Assert.AreEqual("Initial Value", obj.TestProperty);
            Assert.IsTrue(!obj.IsChanged);
        }

        [TestMethod()]
        public void EndEditTest()
        {
            var obj = new TestChangeTracking();
            obj.TestProperty = "Initial Value";
            obj.BeginEdit();
            obj.TestProperty = "Changed Value";
            obj.EndEdit();

            Assert.AreEqual("Changed Value", obj.TestProperty);
        }

        [TestMethod()]
        public void PropertyChangedEventTest()
        {
            var obj = new TestChangeTracking();
            bool eventRaised = false;
            obj.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(obj.TestProperty))
                {
                    eventRaised = true;
                }
            };

            obj.TestProperty = "New Value";

            Assert.IsTrue(eventRaised);
        }

        #endregion Methods

        #region Classes

        public class TestChangeTracking : ChangeTrackingBase
        {
            #region Fields

            private string _testProperty;

            #endregion Fields

            #region Properties

            public string TestProperty
            {
                get => _testProperty;
                set
                {
                    _testProperty = value;
                    OnPropertyChanged(nameof(TestProperty));
                }
            }

            #endregion Properties
        }

        #endregion Classes
    }
}