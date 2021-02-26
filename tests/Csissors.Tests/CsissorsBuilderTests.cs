using Csissors;
using Csissors.Attributes;
using Csissors.Repository;
using Csissors.Schedule;
using Csissors.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Csisors.Tests
{
    public static class Extensions
    {
        public static CsissorsBuilder AddMockRepository(this CsissorsBuilder builder)
        {
            var repositoryFactory = new Mock<IRepositoryFactory>();
            repositoryFactory.Setup(x => x.CreateRepositoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<IRepository>());

            return builder.ConfigureServices(services => services.AddSingleton(repositoryFactory.Object));
        }
    }

    public class CsissorsBuilderTests
    {
        class MockContainer
        {
            [CsissorsDynamicTask]
            public async Task Foo()
            {

            }

            [CsissorsTask(Days = 1)]
            public async Task Bar()
            {

            }
        }

        [Fact]
        public async Task BuildAsync_ShouldBuildDynamicTasks()
        {
            // arrange
            var sut = new CsissorsBuilder()
                .AddMockRepository()
                .AddTaskContainer<MockContainer>();

            // act
            var context = await sut.BuildAsync(CancellationToken.None);

            // assert
            context.Tasks.DynamicTasks.Should().HaveCount(1);
            context.Tasks.DynamicTasks[0].Name.Should().Be("Foo");
        }

        [Fact]
        public async Task BuildAsync_ShouldBuildStaticTasks()
        {
            // arrange
            var sut = new CsissorsBuilder()
                .AddMockRepository()
                .AddTaskContainer<MockContainer>();

            var expectedConfiguration = new TaskConfiguration(
                new IntervalSchedule(TimeSpan.FromDays(1), false),
                FailureMode.None,
                ExecutionMode.AtLeastOnce,
                TimeSpan.FromSeconds(60),
                new Dictionary<string, object?>()
            );

            // act
            var context = await sut.BuildAsync(CancellationToken.None);

            // assert
            context.Tasks.StaticTasks.Should().HaveCount(1);

            var task = context.Tasks.StaticTasks[0];
            task.Name.Should().Be("Bar");
            task.Configuration.Should().BeEquivalentTo(expectedConfiguration,
                (config) => config.RespectingRuntimeTypes());
        }

        [Fact]
        public async Task BuildAsync_ShouldBuildDynamicTasks_Delegate()
        {
            // arrange
            var sut = new CsissorsBuilder()
                .AddMockRepository()
                .AddDynamicTask("mock", async () => throw new IOException());

            // act
            var context = await sut.BuildAsync(CancellationToken.None);

            // assert
            context.Tasks.DynamicTasks.Should().HaveCount(1);
            var task = context.Tasks.DynamicTasks[0];
            task.Name.Should().Be("mock");
            await Assert.ThrowsAsync<IOException>(() => task.ExecuteAsync(Mock.Of<ITaskContext>()));
        }

        [Fact]
        public async Task BuildAsync_ShouldBuildStaticTasks_Delegate()
        {
            // arrange
            var expectedConfiguration = new TaskConfiguration(
                new IntervalSchedule(TimeSpan.FromDays(1), false),
                FailureMode.None,
                ExecutionMode.AtLeastOnce,
                TimeSpan.FromSeconds(60),
                new Dictionary<string, object?>()
            );

            var sut = new CsissorsBuilder()
                .AddMockRepository()
                .AddTask("mock", expectedConfiguration, async () => throw new IOException());

            // act
            var context = await sut.BuildAsync(CancellationToken.None);

            // assert
            context.Tasks.StaticTasks.Should().HaveCount(1);

            var task = context.Tasks.StaticTasks[0];
            task.Name.Should().Be("mock");
            task.Configuration.Should().BeSameAs(expectedConfiguration);
            await Assert.ThrowsAsync<IOException>(() => task.ExecuteAsync(Mock.Of<ITaskContext>()));
        }
    }
}