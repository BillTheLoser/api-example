using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Pnnl.Api.Approvals.Data.Tests
{
    [TestClass]
    public class RoutingItemTest
    {
        private readonly RoutingItem _baseModel;

        public RoutingItemTest()
        {
            var mock = new Mock<RoutingItem> { CallBase = true };
            _baseModel = mock.Object;

        }

        private RoutingItem CreateTestItem()
        {
            var testItem = new RoutingItem {
                DocumentTypeName = "Required"
                ,ApplicationItemId = 12345
                , BeneficiaryHanfordId = "1234567"
                , OriginatorHanfordId = "1234567"
                , DocumentEditUrl = null
                , DocumentId = "Required"
                , DocumentTitle = "Required"
                , RecordsClassification = null
                , RecordsContainer = null
                , SubmitUserHanfordId = "1234567"
                , Document = new RoutingDocument() { }
            };
            return testItem;
        }

        private bool ValidateObject(RoutingItem routingItem)
        {

            ValidationContext validationContext = new ValidationContext(routingItem);
            List<ValidationResult> errors = new List<ValidationResult>();

            return Validator.TryValidateObject(routingItem, validationContext, errors);

        }

        [TestMethod]
        public void RoutingItem_IsValid()
        {
            RoutingItem routingItem = CreateTestItem();
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void RoutingItem_EmptyBeneficaryId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.BeneficiaryHanfordId = "";
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_EmptyOriginatorHanfordId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.OriginatorHanfordId = "";
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_EmptyLastChangeHanfordId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.SubmitUserHanfordId = "";
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_EmptyDocumentTitle_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.DocumentTitle = "";
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_EmptyDocumentId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.DocumentId = "";
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_NullApplicationItemId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.ApplicationItemId = null;
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_EmptyDocumentTypeName_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.DocumentTypeName = "";
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_NullBeneficaryId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.BeneficiaryHanfordId = null;
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_NullOriginatorHanfordId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.OriginatorHanfordId = null;
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_NullLastChangeHanfordId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.SubmitUserHanfordId = null;
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_NullDocumentTitle_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.DocumentTitle = null;
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_NullDocumentId_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.DocumentId = null;
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RoutingItem_NullDocumentTypeName_IsInValid()
        {
            RoutingItem routingItem = CreateTestItem();
            routingItem.DocumentTypeName = null;
            
            bool result = ValidateObject(routingItem);
            Assert.AreEqual(false, result);
        }
    }
}
