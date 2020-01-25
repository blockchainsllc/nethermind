//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using Nethermind.Core;
using Nethermind.Core.Test.Builders;
using Nethermind.Evm.Tracing.Proofs;
using Nethermind.Store;
using NUnit.Framework;

namespace Nethermind.Evm.Test.Tracing
{
    [TestFixture]
    public class ProofTxTracerTests : VirtualMachineTestsBase
    {
        [Test]
        public void Can_trace_sender_recipient_miner()
        {
            byte[] code = Prepare.EvmCode
                .PushData(SampleHexData1)
                .Done;

            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(3, trace.Accounts.Count, "count");
            Assert.True(trace.Accounts.Contains(Sender));
            Assert.True(trace.Accounts.Contains(Recipient));
            Assert.True(trace.Accounts.Contains(Miner));
        }
        
        [Test]
        public void Can_trace_sender_recipient_miner_when_all_are_same()
        {
            byte[] code = Prepare.EvmCode
                .PushData(SampleHexData1)
                .Done;

            SenderRecipientAndMiner addresses = new SenderRecipientAndMiner();
            addresses.RecipientKey = SenderKey;
            addresses.MinerKey = SenderKey;
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(addresses, code);
            Assert.AreEqual(1, trace.Accounts.Count, "count");
            Assert.True(trace.Accounts.Contains(Sender));
        }
        
        [Test]
        public void Can_trace_touch_only_null_accounts()
        {
            byte[] code = Prepare.EvmCode
                .PushData(SampleHexData1)
                .PushData(TestItem.AddressC.Bytes)
                .Op(Instruction.BALANCE)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(4, trace.Accounts.Count, "count");
            Assert.True(trace.Accounts.Contains(TestItem.AddressC));
        }
        
        [Test]
        public void Can_trace_touch_only_preexisting_accounts()
        {
            TestState.CreateAccount(TestItem.AddressC, 100);
            TestState.Commit(Spec);
            
            byte[] code = Prepare.EvmCode
                .PushData(SampleHexData1)
                .PushData(TestItem.AddressC.Bytes)
                .Op(Instruction.BALANCE)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(4, trace.Accounts.Count, "count");
            Assert.True(trace.Accounts.Contains(TestItem.AddressC));
        }
        
        [Test]
        public void Can_trace_touch_only_null_miner_accounts()
        {
            byte[] code = Prepare.EvmCode
                .PushData(SampleHexData1)
                .PushData(SenderRecipientAndMiner.Default.Miner.Bytes)
                .Op(Instruction.BALANCE)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(3, trace.Accounts.Count, "count");
        }
        
        [Test]
        public void Can_trace_blockhash()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x01")
                .Op(Instruction.BLOCKHASH)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(1, trace.BlockHashes.Count, "count");
        }
        
        [Test]
        public void Can_trace_multiple_blockhash()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x01")
                .Op(Instruction.BLOCKHASH)
                .PushData("0x02")
                .Op(Instruction.BLOCKHASH)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(2, trace.BlockHashes.Count, "count");
        }
        
        [Test]
        public void Can_trace_result()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x03")
                .PushData("0x00")
                .Op(Instruction.RETURN)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(3, trace.Output.Length);
        }

        [Test]
        public void Can_trace_storage_read()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x01")
                .Op(Instruction.SLOAD)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            
            Assert.AreEqual(1, trace.Storages.Count);
            Assert.True(trace.Storages.Contains(new StorageAddress(SenderRecipientAndMiner.Default.Recipient, 1)));
        }
        
        [Test]
        public void Can_trace_storage_write()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x01")
                .PushData("0x02")
                .Op(Instruction.SSTORE)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(1, trace.Storages.Count);
            Assert.True(trace.Storages.Contains(new StorageAddress(SenderRecipientAndMiner.Default.Recipient, 2)));
        }
        
        [Test]
        public void Can_trace_multiple_storage_writes()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x01")
                .PushData("0x02")
                .Op(Instruction.SSTORE)
                .PushData("0x03")
                .PushData("0x04")
                .Op(Instruction.SSTORE)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(2, trace.Storages.Count);
            Assert.True(trace.Storages.Contains(new StorageAddress(SenderRecipientAndMiner.Default.Recipient, 2)));
            Assert.True(trace.Storages.Contains(new StorageAddress(SenderRecipientAndMiner.Default.Recipient, 4)));
        }
        
        [Test]
        public void Multiple_write_to_same_storage_can_be_traced_without_issues()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x01")
                .PushData("0x02")
                .Op(Instruction.SSTORE)
                .PushData("0x01")
                .PushData("0x02")
                .Op(Instruction.SSTORE)
                .Done;
            
            (ProofTxTracer trace, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(1, trace.Storages.Count);
            Assert.True(trace.Storages.Contains(new StorageAddress(SenderRecipientAndMiner.Default.Recipient, 2)));
        }
        
        [Test]
        public void Can_trace_on_failure()
        {
            byte[] code = Prepare.EvmCode
                .PushData("0x03")
                .PushData("0x00")
                .PushData("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")
                .PushData("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")
                .Op(Instruction.MSTORE)
                .Op(Instruction.RETURN)
                .Done;
            
            (ProofTxTracer tracer, Block block, Transaction tx) = ExecuteAndTraceProofCall(SenderRecipientAndMiner.Default, code);
            Assert.AreEqual(3, tracer.Accounts.Count);
            Assert.AreEqual(0, tracer.Output.Length);
        }

        protected (ProofTxTracer trace, Block block, Transaction tx) ExecuteAndTraceProofCall(SenderRecipientAndMiner addresses, params byte[] code)
        {
            (Block block, Transaction transaction) = PrepareTx(BlockNumber, 100000, code, addresses);
            ProofTxTracer tracer = new ProofTxTracer();
            _processor.Execute(transaction, block.Header, tracer);
            return (tracer, block, transaction);
        }
    }
}