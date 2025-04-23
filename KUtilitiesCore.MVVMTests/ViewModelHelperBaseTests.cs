using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KUtilitiesCore.Data;

namespace KUtilitiesCore.MVVM.Tests
{
    [TestClass()]
    public class ViewModelHelperBaseTests
    {
        [TestMethod()]
        public void ViewModelHelperBase_HasError()
        {
            MyClass myClass = new MyClass();
            myClass.OnLoaded();
            myClass.Age = 0;
            
            Assert.IsTrue(myClass.HasValidationErrors);
        }
        public class MyClass : ViewModelHelperBase
        {
            public override object Title => "Prueba";
            int age;
            [Range(10, 15)]
            public int Age 
            { get => age; set
                {
                    this.SetVMValue(ref age, value);
                } }
            public override void OnLoaded()
            {
                age = 10;
            }

            protected override void OnDestroy()
            {
                
            }

            protected override void UpdateCommands()
            {
                
            }
        }
    }
}