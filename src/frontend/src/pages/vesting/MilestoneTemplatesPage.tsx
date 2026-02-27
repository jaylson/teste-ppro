import { useState } from 'react';
import { Layers, Info } from 'lucide-react';
import { MilestoneTemplateList } from '@/components/vesting';
import { CreateMilestoneTemplateForm } from '@/components/vesting';
import { useClientStore } from '@/stores/clientStore';
import type { MilestoneTemplate } from '@/types';

export default function MilestoneTemplatesPage() {
  const { selectedCompanyId } = useClientStore();
  const [formOpen, setFormOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<MilestoneTemplate | undefined>(undefined);

  const handleCreateNew = () => {
    setEditTarget(undefined);
    setFormOpen(true);
  };

  const handleEdit = (template: MilestoneTemplate) => {
    setEditTarget(template);
    setFormOpen(true);
  };

  const handleClose = () => {
    setFormOpen(false);
    setEditTarget(undefined);
  };

  if (!selectedCompanyId) {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-3 text-gray-500">
        <Layers className="w-10 h-10 text-gray-300" />
        <p className="text-sm">Selecione uma empresa para gerenciar os templates de milestone.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <Layers className="w-7 h-7 text-indigo-600" />
            Templates de Milestone
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Defina critérios de performance reutilizáveis para associar a grants de vesting.
          </p>
        </div>
      </div>

      {/* Info Banner */}
      <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-4 flex gap-3">
        <Info className="w-5 h-5 text-indigo-500 shrink-0 mt-0.5" />
        <div className="text-sm text-indigo-800">
          <p className="font-medium mb-1">Como funcionam os Templates de Milestone?</p>
          <p>
            Crie templates reutilizáveis com critérios como metas de receita, crescimento de
            usuários ou outros KPIs. Ao criar ou editar um grant, você poderá associar esses
            templates para definir as metas de performance que desbloqueiam a aceleração do vesting.
          </p>
          <p className="mt-1">
            Para ver e registrar progresso nos milestones de um grant específico, acesse{' '}
            <strong>Vesting → Grants</strong> e clique no grant desejado.
          </p>
        </div>
      </div>

      {/* Main List */}
      <MilestoneTemplateList
        companyId={selectedCompanyId}
        onCreateNew={handleCreateNew}
        onEdit={handleEdit}
      />

      {/* Create / Edit Modal */}
      {formOpen && (
        <CreateMilestoneTemplateForm
          companyId={selectedCompanyId}
          template={editTarget}
          onClose={handleClose}
          onSuccess={handleClose}
        />
      )}
    </div>
  );
}
