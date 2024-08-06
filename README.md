### Домашняя работа. Написать тесты к своему проекту и добавить их прогон в CI

#### 1. Если партнер не найден, то также нужно выдать ошибку 404;
```cs
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
```
#### 2. Если партнер заблокирован, то есть поле IsActive=false в классе Partner, то также нужно выдать ошибку 400.
 
```cs
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
```
#### 3. Если партнеру выставляется лимит, то мы должны обнулить количество промокодов, которые партнер выдал NumberIssuedPromoCodes, если лимит закончился, то количество не обнуляется;
```cs
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
```
#### 4. При установке лимита нужно отключить предыдущий лимит;
```cs
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

```
#### 5. Лимит должен быть больше 0;
```cs
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
```
#### 6. Нужно убедиться, что сохранили новый лимит в базу данных (это нужно проверить Unit-тестом);
```cs
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
```

#### Результаты:
<image src="images/res.png" alt="result">

#### Прогон в GitHub:
<image src="images/ci.png" alt="ci_github">