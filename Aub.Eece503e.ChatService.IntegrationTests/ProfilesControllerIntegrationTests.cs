using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.Datacontracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Aub.Eece503e.ChatService.IntegrationTests
{
    public class ProfilesControllerIntegrationTests : IClassFixture<ProfileIntegrationTestFixture>, IAsyncLifetime 
    {
        private readonly IChatServiceClient _chatServiceClient;
        private readonly Random _rand = new Random();

        private readonly ConcurrentBag<Profile> _profilesToCleanup = new ConcurrentBag<Profile>();

        public ProfilesControllerIntegrationTests(ProfileIntegrationTestFixture fixture)
        {
            _chatServiceClient = fixture.ProfileServiceClient;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var tasks = new List<Task>();
            foreach (var profile in _profilesToCleanup)
            {
                var task = _chatServiceClient.DeleteProfile(profile.Username);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task PostGetProfileWithImageId()
        {

            var profile = CreateRandomProfile();
            await AddProfile(profile);

            var fetchedProfile = await _chatServiceClient.GetProfile(profile.Username);
            Assert.Equal(profile, fetchedProfile);
        }

        [Fact]
        public async Task PostGetProfileWithoutImageId()
        {

            var profile = CreateRandomProfile();
            profile.ProfilePictureId = Guid.NewGuid().ToString();
            await AddProfile(profile);

            var fetchedProfile = await _chatServiceClient.GetProfile(profile.Username);
            Assert.Equal(profile, fetchedProfile);
        }

        [Theory]
        [InlineData(null, "Joe", "Daniels")]
        [InlineData("fMax", null, "Daniels")]
        [InlineData("fMax", "Joe", null)]
        [InlineData("", "Joe", "Daniels")]
        [InlineData("fMax", "", "Daniels")]
        [InlineData("fMax", "Joe", "")]
        public async Task PostInvalidProfile(string username, string firstname, string lastname)
        {
            var profile = new Profile
            {
                Username = username,
                Firstname = firstname,
                Lastname = lastname
            };
            var e = await Assert.ThrowsAsync<ProfileServiceException>(() => _chatServiceClient.AddProfile(profile));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingProfile()
        {
            string randomUsername = CreateRandomString();
            var e = await Assert.ThrowsAsync<ProfileServiceException>(() => _chatServiceClient.GetProfile(randomUsername));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Fact]
        public async Task AddProfileThatAlreadyExists()
        {
            var profile = CreateRandomProfile();
            await AddProfile(profile);

            var e = await Assert.ThrowsAsync<ProfileServiceException>(() => AddProfile(profile));
            Assert.Equal(HttpStatusCode.Conflict, e.StatusCode);
        }

        private async Task AddProfile(Profile profile)
        {
            await _chatServiceClient.AddProfile(profile);
            _profilesToCleanup.Add(profile);
        }

        [Fact]
        public async Task UpdateExistingProfile()
        {
            var profile = CreateRandomProfile();
            await AddProfile(profile);

            profile.Firstname = CreateRandomString();
            profile.Lastname = CreateRandomString();
            await _chatServiceClient.UpdateProfile(profile.Username,profile);
            var fetchedProfile = await _chatServiceClient.GetProfile(profile.Username);
            Assert.Equal(profile, fetchedProfile);
        }

        [Fact]
        public async Task UpdateNonExistingProfile()
        {
            var profile = CreateRandomProfile();
            var e = await Assert.ThrowsAsync<ProfileServiceException>(() => _chatServiceClient.UpdateProfile(profile.Username, profile));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Theory]
        [InlineData("", "Dany")]
        [InlineData(" ", "Dany")]
        [InlineData(null, "Dany")]
        [InlineData("Joe", "")]
        [InlineData("Joe", " ")]
        [InlineData("Joe", null)]
        public async Task UpdateProfileWithInvalidProperties(string firstname, string lastname)
        {
            var profile = CreateRandomProfile();
            await AddProfile(profile);
            profile.Firstname = firstname;
            profile.Lastname = lastname;
            var e = await Assert.ThrowsAsync<ProfileServiceException>(() => _chatServiceClient.UpdateProfile(profile.Username,profile));
            Assert.Equal(HttpStatusCode.BadRequest, e.StatusCode);
        }

        [Fact]
        public async Task DeleteProfile()
        {
            var profile = CreateRandomProfile();
            await _chatServiceClient.AddProfile(profile);
            await _chatServiceClient.DeleteProfile(profile.Username);
            var e = await Assert.ThrowsAsync<ProfileServiceException>(() => _chatServiceClient.GetProfile(profile.Username));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingProfile()
        {
            var profile = CreateRandomProfile();
            var e = await Assert.ThrowsAsync<ProfileServiceException>(() => _chatServiceClient.DeleteProfile(profile.Username));
            Assert.Equal(HttpStatusCode.NotFound, e.StatusCode);
        }

        private static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        private Profile CreateRandomProfile()
        {
            string username = CreateRandomString();
            string firstname = CreateRandomString();
            var profile = new Profile
            {
                Username = username,
                Firstname = firstname,
                Lastname = "Smith"
            };
            return profile;
        }


    }
}
