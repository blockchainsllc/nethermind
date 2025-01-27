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

using System;
using System.Collections.Generic;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Dirichlet.Numerics;
using Nethermind.Store;

namespace Nethermind.Evm.Tracing.Proofs
{
    public class ProofTxTracer : ITxTracer
    {
        public HashSet<Address> Accounts { get; } = new HashSet<Address>();

        public HashSet<StorageCell> Storages { get; } = new HashSet<StorageCell>();

        public HashSet<Keccak> BlockHashes { get; } = new HashSet<Keccak>();

        public byte[] Output { get; private set; }

        public bool IsTracingBlockHash => true;
        public bool IsTracingReceipt => true;
        public bool IsTracingActions => false;
        public bool IsTracingOpLevelStorage => false;
        public bool IsTracingMemory => false;
        public bool IsTracingInstructions => false;
        public bool IsTracingCode => false;
        public bool IsTracingStack => false;
        public bool IsTracingState => true;

        public void ReportActionEnd(long gas, Address deploymentAddress, byte[] deployedCode)
        {
            throw new NotSupportedException();
        }

        public void ReportBlockHash(Keccak blockHash)
        {
            BlockHashes.Add(blockHash);
        }

        public void ReportByteCode(byte[] byteCode)
        {
            throw new NotSupportedException();
        }

        public void ReportRefundForVmTrace(long refund, long gasAvailable)
        {
            throw new NotSupportedException();
        }

        public void ReportRefund(long refund)
        {
            throw new NotSupportedException();
        }

        public void ReportBalanceChange(Address address, UInt256? before, UInt256? after)
        {
            Accounts.Add(address);
        }

        public void ReportCodeChange(Address address, byte[] before, byte[] after)
        {
            Accounts.Add(address);
        }

        public void ReportNonceChange(Address address, UInt256? before, UInt256? after)
        {
            Accounts.Add(address);
        }

        public void ReportStorageChange(StorageCell storageCell, byte[] before, byte[] after)
        {
            // implicit knowledge here that if we read storage then for sure we have at least asked for the account's balance
            // and so we do not need to add account to Accounts
            Storages.Add(storageCell);
        }
        
        public void ReportStorageRead(StorageCell storageCell)
        {
            // implicit knowledge here that if we read storage then for sure we have at least asked for the account's balance
            // and so we do not need to add account to Accounts
            Storages.Add(storageCell);
        }
        
        public void ReportAccountRead(Address address)
        {
            Accounts.Add(address);
        }

        public void MarkAsSuccess(Address recipient, long gasSpent, byte[] output, LogEntry[] logs, Keccak stateRoot = null)
        {
            Output = output;
        }

        public void MarkAsFailed(Address recipient, long gasSpent, byte[] output, string error, Keccak stateRoot = null)
        {
            Output = output;
        }

        public void StartOperation(int depth, long gas, Instruction opcode, int pc)
        {
            throw new NotSupportedException();
        }

        public void ReportOperationError(EvmExceptionType error)
        {
            throw new NotSupportedException();
        }

        public void ReportOperationRemainingGas(long gas)
        {
            throw new NotSupportedException();
        }

        public void SetOperationStack(List<string> stackTrace)
        {
            throw new NotSupportedException();
        }

        public void ReportStackPush(Span<byte> stackItem)
        {
            throw new NotSupportedException();
        }

        public void SetOperationMemory(List<string> memoryTrace)
        {
            throw new NotSupportedException();
        }

        public void SetOperationMemorySize(ulong newSize)
        {
            throw new NotSupportedException();
        }

        public void ReportMemoryChange(long offset, Span<byte> data)
        {
            throw new NotSupportedException();
        }

        public void ReportStorageChange(Span<byte> key, Span<byte> value)
        {
            throw new NotSupportedException();
        }

        public void SetOperationStorage(Address address, UInt256 storageIndex, byte[] newValue, byte[] currentValue)
        {
            throw new NotSupportedException();
        }

        public void ReportSelfDestruct(Address address, UInt256 balance, Address refundAddress)
        {
            throw new NotSupportedException();
        }

        public void ReportAction(long gas, UInt256 value, Address @from, Address to, byte[] input, ExecutionType callType, bool isPrecompileCall = false)
        {
            throw new NotSupportedException();
        }

        public void ReportActionEnd(long gas, byte[] output)
        {
            throw new NotSupportedException();
        }

        public void ReportActionError(EvmExceptionType evmExceptionType)
        {
            throw new NotSupportedException();
        }
    }
}