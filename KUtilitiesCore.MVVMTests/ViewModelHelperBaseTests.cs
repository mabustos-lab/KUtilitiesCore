using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KUtilitiesCore.Data;
using System.ComponentModel;

namespace KUtilitiesCore.MVVMTests
{
    [TestClass()]
    public class ViewModelHelperBaseTests
    {
        [TestMethod()]
        public void ViewModelHelperBase_HasError()
        {
            MyClass myClass = new();
            myClass.OnLoaded();
            myClass.Age = 0;
            
            Assert.IsTrue(myClass.HasValidationErrors);
        }
        public class MyClass : ViewModelHelperBase
        {
            public override string Title => "Prueba";
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

            public override void OnDestroy()
            {
                
            }

            protected override void UpdateCommands()
            {
                
            }

            protected override void OnClose(CancelEventArgs e)
            {
               
            }
        }
    }
}