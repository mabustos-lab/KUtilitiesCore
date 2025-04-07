using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Tracking.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace KUtilitiesCoreTests.Tracking.Collection
{
    [TestClass()]
    public class TrackedCollectionTests
    {
        #region Methods

        [TestMethod()]
        public void ClearItemsTest()
        {
            var collection = new TrackedCollection<TestEntity>();
            var entity = new TestEntity { Name = "Test" };
            collection.Add(entity);
            collection.Clear();

            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod()]
        public void GetTrackedItemsTest()
        {
            var collection = new TrackedCollection<TestEntity>();
            var entity = new TestEntity { Name = "Test" };
            collection.Add(entity);

            var trackedItems = collection.GetTrackedItems();
            Assert.AreEqual(1, trackedItems.Count);
            Assert.AreEqual(entity, trackedItems[0].Entity);
            Assert.AreEqual(TrackedStatus.Added, trackedItems[0].Status);
        }

        [TestMethod()]
        public void InsertItemTest()
        {
            var collection = new TrackedCollection<TestEntity>();
            var entity = new TestEntity { Name = "Test" };
            collection.Add(entity);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(entity, collection[0]);
        }

        [TestMethod()]
        public void OnItemValueChangedTest()
        {
            var collection = new TrackedCollection<TestEntity>();
            var entity = new TestEntity { Name = "Test" };
            collection.BeginInsertUnmodifiedEntities();
            collection.Add(entity);
            collection.EndInsertUnmodifiedEntities();
            entity.Name = "Changed";

            var trackedItems = collection.GetTrackedItems();
            Assert.AreEqual(TrackedStatus.Modified, trackedItems[0].Status);
        }

        [TestMethod()]
        public void RemoveItemTest()
        {
            var collection = new TrackedCollection<TestEntity>();
            var entity = new TestEntity { Name = "Test" };
            collection.Add(entity);
            collection.Remove(entity);

            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod()]
        public void SetItemTestOnAddedItems()
        {
            var collection = new TrackedCollection<TestEntity>();
            var entity1 = new TestEntity { Name = "Test1" };
            var entity2 = new TestEntity { Name = "Test2" };
            collection.Add(entity1);
            collection[0] = entity2;

            Assert.AreEqual(1, collection.GetTrackedItems().Count);
            
        }

        [TestMethod()]
        public void SetItemTestOnUnchangedItems()
        {
            var collection = new TrackedCollection<TestEntity>();
            var entity1 = new TestEntity { Name = "Test1" };
            var entity2 = new TestEntity { Name = "Test2" };
            collection.BeginInsertUnmodifiedEntities();
            collection.Add(entity1);
            collection.EndInsertUnmodifiedEntities();
            collection[0] = entity2;

            Assert.AreEqual(2, collection.GetTrackedItems().Count);
            
        }

        [TestMethod()]
        public void TrackedCollectionTest()
        {
            var collection = new TrackedCollection<TestEntity>();
            Assert.IsNotNull(collection);
        }

        #endregion Methods

        #region Classes

        public class TestEntity : INotifyPropertyChanged
        {
            #region Fields

            private string _name;

            #endregion Fields

            #region Events

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            #endregion Properties

            #region Methods

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion Methods
        }

        #endregion Classes
    }
}