﻿namespace Nancy.Tests.Unit.Validation
{
    using System;
    using FakeItEasy;
    using Nancy.Validation;
    using Xunit;
    using Nancy.Tests.Fakes;

    public class ModuleExtensionsFixture
    {
        private readonly IModelValidatorLocator validatorLocator;
        private readonly FakeNancyModule subject;

        public ModuleExtensionsFixture()
        {
            this.validatorLocator = A.Fake<IModelValidatorLocator>();
            this.subject = new FakeNancyModule
            {
                ValidatorLocator = validatorLocator
            };
        }

        [Fact]
        public void Should_be_valid_when_no_validator_exists_for_type()
        {
            // Given, When
            var result = subject.Validate<FakeModel>(new FakeModel());

            // Then
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Should_be_valid_when_a_validator_exists_and_the_instance_is_valid()
        {
            // Given
            var validator = A.Fake<IModelValidator>();
            A.CallTo(() => validator.Validate(A<object>.Ignored)).Returns(ModelValidationResult.Valid);
            A.CallTo(() => validatorLocator.GetValidatorForType(A<Type>.Ignored)).Returns(validator);

            // When
            var result = subject.Validate<FakeModel>(new FakeModel());

            // Then
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void Should_be_invalid_when_a_validator_exists_and_the_instance_is_valid()
        {
            // Given
            var validator = A.Fake<IModelValidator>();
            A.CallTo(() => validator.Validate(A<object>.Ignored)).Returns(new ModelValidationResult(new[] { new ModelValidationError("blah", s => "blah") }));
            A.CallTo(() => validatorLocator.GetValidatorForType(A<Type>.Ignored)).Returns(validator);

            // When
            var result = subject.Validate<FakeModel>(new FakeModel());

            // Then
            result.IsValid.ShouldBeFalse();
        }

        private class FakeModel
        {
        }
    }
}