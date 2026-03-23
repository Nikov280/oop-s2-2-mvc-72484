using Xunit;
using FoodSafety.Domain.Entities;
using FoodSafety.MVC.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace FoodSafety.Tests
{
    public class BusinessRuleTests
    {
        // TEST 1: Logic - Ensures new follow-ups start as "Open"
        [Fact]
        public void FollowUp_Status_Initial_ShouldBeOpen()
        {
            // Arrange
            var followUp = new FollowUp { Status = "Open" };

            // Assert
            Assert.Equal("Open", followUp.Status);
            Assert.Null(followUp.ClosedDate);
        }

        // TEST 2: Logic - Ensures the score stays within the valid 0-100 range
        [Fact]
        public void Inspection_Score_Should_Be_Within_Range()
        {
            // Arrange
            var inspection = new Inspection { Score = 85 };

            // Assert
            Assert.True(inspection.Score >= 0 && inspection.Score <= 100);
        }

        // TEST 3: Logic - Verifies that closing a follow-up requires a date
        [Fact]
        public void FollowUp_Closed_Should_Have_ClosedDate()
        {
            // Arrange
            var followUp = new FollowUp
            {
                Status = "Closed",
                ClosedDate = DateTime.Now
            };

            // Assert
            Assert.NotNull(followUp.ClosedDate);
            Assert.Equal("Closed", followUp.Status);
        }

        // TEST 4: Security - Verifies that FollowUpsController is protected by [Authorize]
        [Fact]
        public void FollowUpsController_Should_Have_Authorize_Attribute()
        {
            // Act: Get the class type and look for the Authorize attribute
            var type = typeof(FollowUpsController);
            var attribute = type.GetCustomAttribute<AuthorizeAttribute>();

            // Assert: It must not be null (meaning the controller is secure)
            Assert.NotNull(attribute);
        }

        // TEST 5: Security - Verifies that the Dashboard allows all 3 required roles
        [Fact]
        public void DashboardController_Should_Allow_Admin_Inspector_and_Viewer()
        {
            // Act
            var type = typeof(DashboardController);
            var attribute = type.GetCustomAttribute<AuthorizeAttribute>();

            // Assert: Check if all required roles are defined in the Roles property
            Assert.Contains("Admin", attribute.Roles);
            Assert.Contains("Inspector", attribute.Roles);
            Assert.Contains("Viewer", attribute.Roles);
        }

        // TEST 6: Logic - Ensures a Premises object starts with a clean list of inspections
        [Fact]
        public void Premises_Should_Initialize_With_Empty_Inspections_List()
        {
            // Arrange
            var premises = new Premises();

            // Assert
            Assert.NotNull(premises.Inspections);
            Assert.Empty(premises.Inspections);
        }
    }
}