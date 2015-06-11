using BugTracker.RestServices.Models.OutputModels;

namespace BugTracker.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using BugTracker.Data.Models;
    using BugTracker.Tests.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BugCommentsIntegrationTests
    {
        [TestMethod]
        public void CreateBugWithOneComment_GetBugComments_ShouldReturnOneComment()
        {
            // Arrange ->
            TestingEngine.CleanDatabase();

            var bugTitle = "new bug";
            var bugDesc = "new desc";
            var httpPostResponse = TestingEngine.SubmitBugHttpPost(bugTitle, bugDesc);
            Assert.AreEqual(HttpStatusCode.Created, httpPostResponse.StatusCode);
            var postedBug = httpPostResponse.Content.ReadAsAsync<Bug>().Result;

            var httpPostResponseFirstComment =
                TestingEngine.SubmitCommentHttpPost(postedBug.Id, "first comment");
            Assert.AreEqual(HttpStatusCode.OK, httpPostResponseFirstComment.StatusCode);
            Thread.Sleep(5);

            var httpPostResponseSecondComment =
                TestingEngine.SubmitCommentHttpPost(postedBug.Id, "second comment");
            Assert.AreEqual(HttpStatusCode.OK, httpPostResponseSecondComment.StatusCode);
            Thread.Sleep(5);

            var httpPostResponseThirdComment =
                TestingEngine.SubmitCommentHttpPost(postedBug.Id, "third comment");
            Assert.AreEqual(HttpStatusCode.OK, httpPostResponseThirdComment.StatusCode);
            Thread.Sleep(5);

            // Act ->
            var httpResponse = TestingEngine
                .HttpClient.GetAsync("/api/bugs/" + postedBug.Id + "/comments")
                .Result;

            // Assert -> check the returned comments
            Assert.AreEqual(HttpStatusCode.OK, httpResponse.StatusCode);
            var bugCommentsFromService = 
                httpResponse.Content
                .ReadAsAsync<List<Comment>>()
                .Result;

            Assert.AreEqual(3, bugCommentsFromService.Count());

            Assert.IsTrue(bugCommentsFromService[2].Id > 0);
            Assert.AreEqual("first comment", bugCommentsFromService[2].Text);
            Assert.AreEqual(null, bugCommentsFromService[2].Author);
            Assert.IsTrue(bugCommentsFromService[2].PublishDate - DateTime.Now < TimeSpan.FromMinutes(1));
            //Assert.AreEqual(postedBug.Id, bugCommentsFromService[2].BugId);

            Assert.IsTrue(bugCommentsFromService[1].Id > 0);
            Assert.AreEqual("second comment", bugCommentsFromService[1].Text);
            Assert.AreEqual(null, bugCommentsFromService[1].Author);
            Assert.IsTrue(bugCommentsFromService[1].PublishDate - DateTime.Now < TimeSpan.FromMinutes(1));
            //Assert.AreEqual(postedBug.Id, bugCommentsFromService[1].BugId);

            Assert.IsTrue(bugCommentsFromService[0].Id > 0);
            Assert.AreEqual("third comment", bugCommentsFromService[0].Text);
            Assert.AreEqual(null, bugCommentsFromService[0].Author);
            Assert.IsTrue(bugCommentsFromService[0].PublishDate - DateTime.Now < TimeSpan.FromMinutes(1));
            //Assert.AreEqual(postedBug.Id, bugCommentsFromService[2].BugId);
        }

        [TestMethod]
        public void GetBugCommentsFromNonExistingBug_ShouldReturnStatusCodeNotFound()
        {
            // Arrange ->
            TestingEngine.CleanDatabase();

            var httpPostResponseFirstComment =
                TestingEngine.SubmitCommentHttpPost(3, "first comment");

            Assert.AreEqual(HttpStatusCode.NotFound, httpPostResponseFirstComment.StatusCode);
        }
    }
}
