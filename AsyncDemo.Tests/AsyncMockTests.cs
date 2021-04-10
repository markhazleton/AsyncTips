using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDemo.Tests
{
    [TestClass]
    public class AsyncMockTests
    {
        [TestMethod]
        public async Task LongRunningCancellableOperation_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var asyncMock = new AsyncMock();
            int loop = 10;
            CancellationToken cancellationToken = default(global::System.Threading.CancellationToken);

            // Act
            var result = await asyncMock.LongRunningCancellableOperation(
                loop,
                cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, 45);
        }

        [TestMethod]
        public async Task LongRunningOperation_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var asyncMock = new AsyncMock();
            int loop = 10;

            // Act
            var result = await asyncMock.LongRunningOperation(loop);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, 45);
        }

        [TestMethod]
        public async Task LongRunningOperationWithCancellationTokenAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var asyncMock = new AsyncMock();
            int loop = 10;
            CancellationToken cancellationToken = default(global::System.Threading.CancellationToken);

            // Act
            var result = await asyncMock.LongRunningOperationWithCancellationTokenAsync(
                loop,
                cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result, 45);
        }
    }
}
