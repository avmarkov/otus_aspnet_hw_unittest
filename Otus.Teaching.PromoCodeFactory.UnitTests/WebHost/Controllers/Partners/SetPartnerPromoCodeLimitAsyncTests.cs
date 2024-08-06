using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;
        private readonly SetPartnerPromoCodeLimitRequest _partnersPromoCodeLimitRequest;
        public SetPartnerPromoCodeLimitAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
            _partnersPromoCodeLimitRequest = fixture.Build<SetPartnerPromoCodeLimitRequest>().Create();
        }

        public Partner CreateBasePartner()
        {
            var partner = new Partner()
            {
                Id = Guid.NewGuid(),
                Name = "NewName",
                IsActive = true,

            };

            partner.PartnerLimits = new List<PartnerPromoCodeLimit> {
                {
                    new PartnerPromoCodeLimit()
                    {
                        Id = Guid.NewGuid(),
                        CreateDate = DateTime.Now,
                        //EndDate = DateTime.Now.AddDays(1),
                        PartnerId = partner.Id,
                        Limit = 100
                    }
                }
            };

            return partner;
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            Partner partner = null;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, _partnersPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange            
            var partner = CreateBasePartner();
            partner.IsActive = false;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ActiveLimitIsNotNull_NumberIssuedPromoCodesIsZero()
        {
            // Arrange
            var partner = CreateBasePartner();
            int num = 10;

            partner.NumberIssuedPromoCodes = num;
            partner.PartnerLimits.ElementAt(0).CancelDate = null;

            _partnersPromoCodeLimitRequest.EndDate = DateTime.Now.AddDays(1);
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ActiveLimitIsNull_NumberIssuedPromoCodesIsNotZero()
        {
            // Arrange
            var partner = CreateBasePartner();
            int num = 10;
            partner.NumberIssuedPromoCodes = num;
            partner.PartnerLimits.ElementAt(0).CancelDate = DateTime.Now;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(num);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ActiveLimitIsNotNull_CancelDateNotBeNull()
        {
            // Arrange
            var partner = CreateBasePartner();
            int num = 10;

            partner.NumberIssuedPromoCodes = num;
            partner.PartnerLimits.ElementAt(0).CancelDate = null;

            _partnersPromoCodeLimitRequest.EndDate = DateTime.Now.AddDays(1);
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            partner.PartnerLimits.ElementAt(0).CancelDate.Should().NotBe(null);
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_LimitIsLessThan0_ReturnsBadRequest()
        {
            // Arrange
            var partner = CreateBasePartner();
            _partnersPromoCodeLimitRequest.Limit = -1;
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);

            // Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _partnersPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SetPartnerPromoCodeLimitAsyncTests_ValidSave_SuccessUpdate()
        {
            // Arrange
            var partner = CreateBasePartner();
            var partnerId = partner.Id;
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();

            _partnersRepositoryMock
                .Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(partner), Times.Once);
        }
    }
}