using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class ProtocolSerializationTest
{
    [UnityTest]
    public IEnumerator TestProtocolSerialization()
    {
        // Arrange
        var dataProvider = new ResourceFileDataProvider();
        var protocolName = "TestProtocol";

        ProtocolDefinition originalProtocol = null;
        bool isCompleted = false;

        // Act
        dataProvider.GetOrCreateProtocolDefinition(protocolName).Subscribe(protocol =>
        {
            originalProtocol = protocol;
            isCompleted = true;
        }, error =>
        {
            Assert.Fail("Failed to load protocol: " + error);
        });

        while (!isCompleted)
        {
            yield return null;
        }

        // Serialize the protocol
        var serializedJson = JsonConversionV2.SerializeProtocol(originalProtocol);

        // Deserialize the protocol
        var deserializedProtocol = JsonConversionV2.DeserializeProtocol(serializedJson);

        // Serialize again to compare
        var reSerializedJson = JsonConversionV2.SerializeProtocol(deserializedProtocol);

        // Assert
        Assert.AreEqual(serializedJson, reSerializedJson, "Serialized JSON does not match after deserialization");

        Debug.Log("Protocol serialization and deserialization test passed");
    }
} 