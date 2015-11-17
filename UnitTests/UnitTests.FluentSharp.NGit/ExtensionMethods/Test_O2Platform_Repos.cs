﻿using System;
using FluentSharp.CoreLib;
using FluentSharp.CoreLib.API;
using FluentSharp.Git;
using FluentSharp.Git.APIs;
using FluentSharp.Git.Utils;
using FluentSharp.Web35.API;
using NUnit.Framework;
using NGit.Transport;

namespace UnitTests.FluentSharp.Git
{
    [TestFixture]
    class Test_O2Platform_Repos
    {
        public string tempFolder;

        public Test_O2Platform_Repos()
        {
            if (Web.Online.isFalse())
                Assert.Ignore("Skiping because we are offline");

            
        }

        [SetUp]
        public void setup()
        {
            tempFolder = "_API_NGit_O2Platform".tempDir();
            Assert.IsTrue(tempFolder.dirExists());
        }

        [TearDown]
        public void teardown()
        {
            tempFolder.folder_Delete();
            Assert.IsFalse(tempFolder.dirExists());
        }
        [Test]
        [Ignore("GitHub.com is down")]
        public void NGit_O2Platform()
        {
            var repoToClone = "UnitTests_TestRepo";
            var pathToRepo  = tempFolder.pathCombine(repoToClone);
            
            Assert.IsTrue (tempFolder.dirExists());
            Assert.IsEmpty(tempFolder.dirs());
            Assert.IsFalse(pathToRepo.dirExists());
            Assert.IsFalse(pathToRepo.isGitRepository());

            API_NGit_O2Platform ngit_O2 = null;

            Action checkRepo = 
                    ()=>{
                            Assert.IsNotNull(ngit_O2);
                            Assert.IsNull   (ngit_O2.Git);
                            Assert.IsNull   (ngit_O2.Repository);

                            ngit_O2.cloneOrPull(repoToClone);

                            Assert.IsTrue   (pathToRepo.dirExists());
                            Assert.IsTrue   (pathToRepo.isGitRepository());

                            ngit_O2.open(pathToRepo);

                            Assert.IsNotNull(ngit_O2.Git);
                            Assert.IsNotNull(ngit_O2.Repository);

                            ngit_O2.nGit().close();
                    };      

            //Test Clone
            ngit_O2 = new API_NGit_O2Platform(tempFolder);
            checkRepo();
            
            //Test Open
            ngit_O2 = new API_NGit_O2Platform(tempFolder);
            checkRepo();

            var result = ngit_O2.delete_Repository_And_Files();
            Assert.IsTrue(result);

            tempFolder.delete_Folder();
            Assert.IsFalse(tempFolder.dirExists());
        }

        [Test]
        [Ignore("GitHub.com is down")]
        public void Clone_Private_Repo_No_Authorization()
        {
            var repoToClone = "UnitTests_TestRepo_Private";
            var pathToRepo = tempFolder.pathCombine(repoToClone);
            Files.deleteFolder(pathToRepo,true);            
            Assert.IsTrue(tempFolder.dirExists());
            Assert.IsEmpty(tempFolder.dirs());
            Assert.IsFalse(pathToRepo.dirExists());
            Assert.IsFalse(pathToRepo.isGitRepository());

            var ngit_O2 = new API_NGit_O2Platform(tempFolder);
            var repositoryUrl = ngit_O2.repositoryUrl(repoToClone);

            Assert.IsNull(ngit_O2.Last_Exception);

            //clone should fail 
            ngit_O2.clone(repositoryUrl, pathToRepo);

            //no git repo should be there (if clone failed
            Assert.IsNotNull(ngit_O2.Last_Exception);
            Assert.IsEmpty(tempFolder.dirs());
            Assert.IsFalse(pathToRepo.dirExists());
            Assert.IsFalse(pathToRepo.isGitRepository());                        
        }

        [Test]//[Ignore("Needs hardcoded credentials")]
        //[Ignore("GitHub.com is down")]
        public void Clone_Private_Repo_With_Authentication_UserName_Pwd()
        {
            var hardCodedbadPWd = "pwd_123456!!!";

            //clone should fail unless the values above are configured to a valid account
            var username = "gitHub_UserName";
            var pwd      = hardCodedbadPWd;
            
            
            var repoToClone = "UnitTests_TestRepo_Private";
            var pathToRepo = tempFolder.pathCombine(repoToClone);
            var ngit_O2 = new API_NGit_O2Platform(tempFolder);

            var repositoryUrl = ngit_O2.repositoryUrl(repoToClone);
            ngit_O2.use_Credential(username, pwd);            

            ngit_O2.clone(repositoryUrl, pathToRepo);
            if (pwd == hardCodedbadPWd) 
            {
                Assert.IsNotNull(ngit_O2.Last_Exception);
                Assert.IsTrue   (ngit_O2.Last_Exception.Message.contains("not authorized"));
            }
            else
            {
                // these will only run when we are using a valid GitHub Account
                Assert.IsNull(ngit_O2.Last_Exception);
                Assert.IsNotEmpty(tempFolder.dirs());
                Assert.IsTrue(pathToRepo.dirExists());
                Assert.IsTrue(pathToRepo.isGitRepository());    
            }
                        
            ngit_O2.delete_Repository_And_Files();

            Assert.IsFalse(pathToRepo.dirExists());
        }

        [Test]        
        public void Clone_Private_Repo_With_Authentication_SSH()
        {            
            var privateKey = @"C:\Users\o2\.ssh\id_rsa";
            var publicKey = @"C:\Users\o2\.ssh\id_rsa.pub";

            if (privateKey.fileExists().isFalse() || publicKey.fileExists().isFalse())
                Assert.Ignore("Ignoring test because test SSH keys were not found");

            var factory = new CustomConfigSessionFactory(publicKey.fileContents(), privateKey.fileContents());
            SshSessionFactory.SetInstance(factory);

            
            var repoToClone = "UnitTests_TestRepo_Private";
            var repositoryUrl = "git@github.com:o2platform/{0}.git".format(repoToClone);
            var pathToRepo = tempFolder.pathCombine(repoToClone);
            var ngit_O2 = new API_NGit_O2Platform(tempFolder);            

            ngit_O2.clone(repositoryUrl, pathToRepo);

            Assert.IsNull(ngit_O2.Last_Exception);
            Assert.IsNotEmpty(tempFolder.dirs());
            Assert.IsTrue(pathToRepo.dirExists());
            Assert.IsTrue(pathToRepo.isGitRepository());

            ngit_O2.delete_Repository_And_Files();

            Assert.IsFalse(pathToRepo.dirExists());
            SshSessionFactory.SetInstance(null);
        }
    }
}
