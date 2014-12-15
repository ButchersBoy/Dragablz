using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Dragablz.Core;
using Microsoft.CSharp;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dragablz.Test
{
    [TestFixture]
    public class CollectionTeaserFixture
    {
        [Test]
        public void WillCreateForList()
        {
            var myList = new ArrayList();

            CollectionTeaser collectionTeaser;
            var result = CollectionTeaser.TryCreate(myList, out collectionTeaser);

            Assert.IsTrue(result);
            Assert.IsNotNull(collectionTeaser);
        }

        [Test]
        public void WillCreateForGenericCollection()
        {
            var myList = MockRepository.GenerateStub<ICollection<string>>();

            CollectionTeaser collectionTeaser;
            var result = CollectionTeaser.TryCreate(myList, out collectionTeaser);

            Assert.IsTrue(result);
            Assert.IsNotNull(collectionTeaser);
        }

        [Test]
        public void WillCreateForCollection()
        {
            var myList = MockRepository.GenerateStub<ICollection>();

            CollectionTeaser collectionTeaser;
            var result = CollectionTeaser.TryCreate(myList, out collectionTeaser);

            Assert.IsFalse(result);
            Assert.IsNull(collectionTeaser);
        }

        [Test]
        public void WillAddForList()
        {
            var myList = new ArrayList();
            CollectionTeaser collectionTeaser;
            Assert.IsTrue(CollectionTeaser.TryCreate(myList, out collectionTeaser));

            collectionTeaser.Add("i am going to type this in, manually, twice.");

            CollectionAssert.AreEquivalent(new[] {"i am going to type this in, manually, twice."}, myList);
            //i didnt really.  i copied and pasted it.
        }

        [Test]
        public void WillRemoveForList()
        {
            var myList = new ArrayList
            {
                1,
                2,
                3,
                4,
                5
            };
            CollectionTeaser collectionTeaser;
            Assert.IsTrue(CollectionTeaser.TryCreate(myList, out collectionTeaser));

            collectionTeaser.Remove(3);

            CollectionAssert.AreEquivalent(new[] {1, 2, 4, 5}, myList);
        }

        [Test]
        public void WillAddForGenericCollection()
        {
            var myList = MockRepository.GenerateStub<ICollection<string>>();
            CollectionTeaser collectionTeaser;
            Assert.IsTrue(CollectionTeaser.TryCreate(myList, out collectionTeaser));

            collectionTeaser.Add("hello");

            myList.AssertWasCalled(c => c.Add("hello"));
            myList.AssertWasNotCalled(c => c.Remove("hello"));
        }

        [Test]
        public void WillRemoveForGenericCollection()
        {
            var myList = MockRepository.GenerateStub<ICollection<string>>();
            CollectionTeaser collectionTeaser;
            Assert.IsTrue(CollectionTeaser.TryCreate(myList, out collectionTeaser));

            collectionTeaser.Remove("bye");

            myList.AssertWasCalled(c => c.Remove("bye"));
            myList.AssertWasNotCalled(c => c.Add("bye"));

        }
    }
}
