﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using Microsoft.VisualStudio.ProjectSystem.Properties;

using Xunit;

#nullable disable

namespace Microsoft.VisualStudio.ProjectSystem.ProjectPropertiesProviders
{
    public class TargetFrameworkValueProviderTests
    {
        private const string TargetFrameworkPropertyName = "TargetFramework";

        private static InterceptedProjectPropertiesProviderBase CreateInstance(FrameworkName configuredTargetFramework)
        {
            var data = new PropertyPageData()
            {
                Category = ConfigurationGeneral.SchemaName,
                PropertyName = ConfigurationGeneral.TargetFrameworkMonikerProperty,
                Value = configuredTargetFramework.FullName
            };

            var project = UnconfiguredProjectFactory.Create();
            var properties = ProjectPropertiesFactory.Create(project, data);
            var delegateProvider = IProjectPropertiesProviderFactory.Create();

            var instancePropertiesMock = IProjectPropertiesFactory
                .MockWithProperty(TargetFrameworkPropertyName);

            var instanceProperties = instancePropertiesMock.Object;
            var instanceProvider = IProjectInstancePropertiesProviderFactory.ImplementsGetCommonProperties(instanceProperties);

            var targetFrameworkProvider = new TargetFrameworkValueProvider(properties);
            var providerMetadata = IInterceptingPropertyValueProviderMetadataFactory.Create(TargetFrameworkPropertyName);
            var lazyArray = new[] { new Lazy<IInterceptingPropertyValueProvider, IInterceptingPropertyValueProviderMetadata>(
                () => targetFrameworkProvider, providerMetadata) };
            return new ProjectFileInterceptedViaSnapshotProjectPropertiesProvider(delegateProvider, instanceProvider, project, lazyArray);
        }

        [Fact]
        public async Task VerifyGetTargetFrameworkPropertyAsync()
        {
            var configuredTargetFramework = new FrameworkName(".NETFramework", new Version(4, 5));
            var expectedTargetFrameworkPropertyValue = (uint)0x40005;
            var provider = CreateInstance(configuredTargetFramework);
            var properties = provider.GetCommonProperties(null);
            var propertyValueStr = await properties.GetEvaluatedPropertyValueAsync(TargetFrameworkPropertyName);
            Assert.True(uint.TryParse(propertyValueStr, out uint propertyValue));
            Assert.Equal(expectedTargetFrameworkPropertyValue, propertyValue);
        }
    }
}
