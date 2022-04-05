using System;
using System.Numerics;
using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract.Framework.Attributes;

namespace NEP17.Examples
{
    // Manifest data is the data that will show on public block explorer like OneGate, Dora, NeoTube
    [ManifestExtra("Author", "Nep17-tutorial")]
    [ManifestExtra("Email", "Nep17@tutorial.org")]
    [ManifestExtra("Description", "This is a NEP17 example")]
    [SupportedStandards("NEP-17")]
    [ContractPermission("*", "onNEP17Payment")]
    public partial class NEP17 : Nep17Token
    {
        // Owner address, must be the same as wallet that will be used to deploy.
        [InitialValue("NM4qCMkFYFKFQeNBwDaBzTAxNS8xchyFnm", ContractParameterType.Hash160)]
        private static readonly UInt160 owner;
        // Prefix is use as a StorageMap Key, we could have different prefix for different purpose.
        private const byte Prefix_Contract = 0x02;
        private static readonly StorageMap ContractMap = new StorageMap(Storage.CurrentContext, Prefix_Contract);
        private static readonly byte[] ownerKey = "owner".ToByteArray(); // Key of owner record that will store in the ContractMap
        // CheckWitness will check that address that used to sign transaction is the same as address passed as argument or not
        private static bool IsOwner() => Runtime.CheckWitness(GetOwner());

        // Set initial and maximum supply at 10 million with 8 decimal points.
        // Please note that smart contract will always deal with integer
        private static readonly ulong InitialSupply = 10_000_000_00000000;

        #region THIS SECTION FOR read methods that return contract data
        public override byte Decimals() => 8; // Token decimal points
        public override string Symbol() => "NEP17"; // Token ticker symbol

        public static UInt160 GetOwner()
        {
            return (UInt160)ContractMap.Get(ownerKey);
        }
        #endregion

        #region THIS SECTION FOR write methods that users can invoke
        // Please note that there is no Transfer method explicitly declare here.
        // Because we use method as is that inherited from Nep17Token class.
        public static new void Burn(UInt160 account, BigInteger amount)
        {
            if (!IsOwner()) throw new InvalidOperationException("No Authorization for Burn!");

            // Dangerous method!! 
            // Only for demontration the burn mechanism, because owner can burn token on any addresses.
            Nep17Token.Burn(account, amount);
        }
        #endregion

        #region THIS SECTION FOR contract management methods
        // Automatically called every time contract has been deployed or updated
        public static void _deploy(object data, bool update)
        {
            // Contract will do nothing if updating
            if (update) return;
            // Save initial owner when first time deploy
            ContractMap.Put(ownerKey, owner);
            // Mint all supply to the owner address at once when contract deployed.
            Nep17Token.Mint(owner, InitialSupply);
        }

        // This method is necessary for contract to be able to update
        // This method can be removed, if you want contract to be unable to updated.
        public static bool Update(ByteString nefFile, string manifest)
        {
            if (!IsOwner()) throw new InvalidOperationException("No Authorization for Update!");
            ContractManagement.Update(nefFile, manifest, null);
            return true;
        }

        // This method is necessary for contract to be able to destroy
        // Similar to Update method, can be removed if you want contract to be permanently on the blockchain.
        public static bool Destroy()
        {
            if (!IsOwner()) throw new InvalidOperationException("No Authorization for Destroy!");
            ContractManagement.Destroy();
            return true;
        }
        #endregion
    }
}