// F3-BLD-FE-002: Contract Builder Step 2 - Add Parties
// File: src/frontend/src/components/contracts/builder/Step2AddParties.tsx
// Author: GitHub Copilot
// Date: 14/02/2026

import React, { useState } from 'react';
import { Users, Plus, Trash2, Mail, User, Tag } from 'lucide-react';
import { Card, Button } from '@/components/ui';
import type { CreateContractPartyRequest } from '@/types/contract.types';

interface Step2AddPartiesProps {
  parties: CreateContractPartyRequest[];
  onUpdate: (parties: CreateContractPartyRequest[]) => void;
}

export const Step2AddParties: React.FC<Step2AddPartiesProps> = ({
  parties,
  onUpdate,
}) => {
  const [editingParty, setEditingParty] = useState<CreateContractPartyRequest>({
    partyName: '',
    partyEmail: '',
    partyType: 'signer',
    sequenceOrder: parties.length + 1,
  });

  const handleAddParty = () => {
    if (!editingParty.partyName || !editingParty.partyEmail) {
      alert('Por favor, preencha o nome e email da parte');
      return;
    }

    // Validar email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(editingParty.partyEmail)) {
      alert('Por favor, insira um email válido');
      return;
    }

    const newParties = [...parties, { ...editingParty }];
    onUpdate(newParties);

    // Reset form
    setEditingParty({
      partyName: '',
      partyEmail: '',
      partyType: 'signer',
      sequenceOrder: newParties.length + 1,
    });
  };

  const handleRemoveParty = (index: number) => {
    const newParties = parties.filter((_, i) => i !== index);
    // Update sequence orders
    const reorderedParties = newParties.map((party, idx) => ({
      ...party,
      sequenceOrder: idx + 1,
    }));
    onUpdate(reorderedParties);
  };

  const handleMoveUp = (index: number) => {
    if (index === 0) return;
    const newParties = [...parties];
    [newParties[index - 1], newParties[index]] = [newParties[index], newParties[index - 1]];
    // Update sequence orders
    const reorderedParties = newParties.map((party, idx) => ({
      ...party,
      sequenceOrder: idx + 1,
    }));
    onUpdate(reorderedParties);
  };

  const handleMoveDown = (index: number) => {
    if (index === parties.length - 1) return;
    const newParties = [...parties];
    [newParties[index], newParties[index + 1]] = [newParties[index + 1], newParties[index]];
    // Update sequence orders
    const reorderedParties = newParties.map((party, idx) => ({
      ...party,
      sequenceOrder: idx + 1,
    }));
    onUpdate(reorderedParties);
  };

  return (
    <div className="space-y-6">
      {/* Description */}
      <div className="text-center mb-8">
        <Users className="w-16 h-16 text-cyan-600 mx-auto mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          Adicione as partes do contrato
        </h3>
        <p className="text-sm text-gray-500">
          Adicione todas as pessoas ou empresas que participarão do contrato. A ordem define a
          sequência de assinatura.
        </p>
      </div>

      {/* Add Party Form */}
      <Card className="p-6">
        <h4 className="text-md font-semibold text-gray-900 mb-4">
          <Plus className="w-5 h-5 inline mr-2" />
          Adicionar Nova Parte
        </h4>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Name */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              <User className="w-4 h-4 inline mr-1" />
              Nome Completo *
            </label>
            <input
              type="text"
              value={editingParty.partyName}
              onChange={(e) =>
                setEditingParty({ ...editingParty, partyName: e.target.value })
              }
              placeholder="Ex: João Silva"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
            />
          </div>

          {/* Email */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              <Mail className="w-4 h-4 inline mr-1" />
              Email *
            </label>
            <input
              type="email"
              value={editingParty.partyEmail}
              onChange={(e) =>
                setEditingParty({ ...editingParty, partyEmail: e.target.value })
              }
              placeholder="joao@empresa.com"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
            />
          </div>

          {/* Party Type */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              <Tag className="w-4 h-4 inline mr-1" />
              Tipo de Participação
            </label>
            <select
              value={editingParty.partyType}
              onChange={(e) =>
                setEditingParty({ ...editingParty, partyType: e.target.value })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-cyan-600"
            >
              <option value="signer">Signatário</option>
              <option value="witness">Testemunha</option>
              <option value="recipient">Destinatário</option>
              <option value="approver">Aprovador</option>
            </select>
          </div>

          {/* Add Button */}
          <div className="flex items-end">
            <Button variant="primary" onClick={handleAddParty} className="w-full">
              <Plus className="w-4 h-4 mr-2" />
              Adicionar Parte
            </Button>
          </div>
        </div>
      </Card>

      {/* Parties List */}
      {parties.length > 0 && (
        <Card className="p-6">
          <h4 className="text-md font-semibold text-gray-900 mb-4">
            Partes Adicionadas ({parties.length})
          </h4>
          <div className="space-y-3">
            {parties.map((party, index) => (
              <div
                key={index}
                className="flex items-center justify-between p-4 bg-gray-50 rounded-lg border border-gray-200"
              >
                <div className="flex-1">
                  <div className="flex items-center gap-3">
                    <div className="flex items-center justify-center w-8 h-8 bg-cyan-100 text-cyan-600 rounded-full font-semibold text-sm">
                      {index + 1}
                    </div>
                    <div>
                      <h5 className="font-medium text-gray-900">{party.partyName}</h5>
                      <p className="text-sm text-gray-600">{party.partyEmail}</p>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <span className="px-3 py-1 bg-blue-100 text-blue-700 text-xs rounded-full">
                    {party.partyType === 'signer'
                      ? 'Signatário'
                      : party.partyType === 'witness'
                      ? 'Testemunha'
                      : party.partyType === 'recipient'
                      ? 'Destinatário'
                      : 'Aprovador'}
                  </span>
                  <div className="flex gap-1">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleMoveUp(index)}
                      disabled={index === 0}
                      className="px-2"
                    >
                      ↑
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleMoveDown(index)}
                      disabled={index === parties.length - 1}
                      className="px-2"
                    >
                      ↓
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleRemoveParty(index)}
                      className="text-red-600 hover:text-red-700 px-2"
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Empty State */}
      {parties.length === 0 && (
        <div className="text-center py-8 border-2 border-dashed border-gray-300 rounded-lg">
          <Users className="w-12 h-12 text-gray-300 mx-auto mb-3" />
          <p className="text-gray-500 text-sm">
            Nenhuma parte adicionada ainda. Use o formulário acima para adicionar.
          </p>
        </div>
      )}

      {/* Info Box */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <p className="text-sm text-blue-800">
          <strong>💡 Dica:</strong> A ordem das partes define a sequência de assinatura do
          contrato. Use as setas para reordenar conforme necessário.
        </p>
      </div>
    </div>
  );
};

export default Step2AddParties;
