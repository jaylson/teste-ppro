import { useState } from 'react';
import { Plus, MoreVertical, Edit2, Trash2, ToggleLeft, ToggleRight } from 'lucide-react';
import type { MilestoneTemplate } from '@/types';
import {
  milestoneCategoryLabels,
  metricTypeLabels,
  vestingAccelerationTypeLabels,
  measurementFrequencyLabels,
} from '@/types';
import {
  useMilestoneTemplates,
  useActivateMilestoneTemplate,
  useDeactivateMilestoneTemplate,
  useDeleteMilestoneTemplate,
} from '@/hooks';

interface MilestoneTemplateListProps {
  companyId: string;
  onCreateNew?: () => void;
  onEdit?: (template: MilestoneTemplate) => void;
}

export function MilestoneTemplateList({
  companyId,
  onCreateNew,
  onEdit,
}: MilestoneTemplateListProps) {
  const [menuOpenId, setMenuOpenId] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  const { data, isLoading } = useMilestoneTemplates({ companyId, page, pageSize: 12 });
  const activate = useActivateMilestoneTemplate();
  const deactivate = useDeactivateMilestoneTemplate();
  const deleteTemplate = useDeleteMilestoneTemplate();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-10 text-gray-400 text-sm">
        Carregando templates...
      </div>
    );
  }

  if (!data || data.items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-gray-400">
        <p className="mb-4 text-sm">Nenhum template criado ainda.</p>
        {onCreateNew && (
          <button
            onClick={onCreateNew}
            className="flex items-center gap-2 px-4 py-2 text-sm bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            <Plus size={15} />
            Criar primeiro template
          </button>
        )}
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <p className="text-sm text-gray-500">
          {data.totalCount} template{data.totalCount !== 1 ? 's' : ''}
        </p>
        {onCreateNew && (
          <button
            onClick={onCreateNew}
            className="flex items-center gap-2 px-3 py-1.5 text-sm bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            <Plus size={14} />
            Novo template
          </button>
        )}
      </div>

      {/* Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        {data.items.map((tpl) => (
          <div
            key={tpl.id}
            className={`bg-white border rounded-xl p-4 relative transition-opacity ${
              tpl.isActive ? 'border-gray-200' : 'border-gray-100 opacity-60'
            }`}
          >
            {/* Status badge */}
            <span
              className={`absolute top-3 right-10 text-xs font-medium px-2 py-0.5 rounded-full ${
                tpl.isActive
                  ? 'bg-green-100 text-green-700'
                  : 'bg-gray-100 text-gray-500'
              }`}
            >
              {tpl.isActive ? 'Ativo' : 'Inativo'}
            </span>

            {/* Menu */}
            <div className="absolute top-2 right-2">
              <button
                className="p-1 rounded hover:bg-gray-100 text-gray-400"
                onClick={() => setMenuOpenId(menuOpenId === tpl.id ? null : tpl.id)}
              >
                <MoreVertical size={15} />
              </button>
              {menuOpenId === tpl.id && (
                <div className="absolute right-0 top-7 bg-white border border-gray-200 rounded-lg shadow-lg z-20 min-w-[160px] py-1">
                  {onEdit && (
                    <button
                      className="w-full flex items-center gap-2 text-left px-4 py-2 text-sm hover:bg-gray-50 text-gray-700"
                      onClick={() => { setMenuOpenId(null); onEdit(tpl); }}
                    >
                      <Edit2 size={13} /> Editar
                    </button>
                  )}
                  {tpl.isActive ? (
                    <button
                      className="w-full flex items-center gap-2 text-left px-4 py-2 text-sm hover:bg-gray-50 text-amber-600"
                      onClick={() => { setMenuOpenId(null); deactivate.mutate(tpl.id); }}
                    >
                      <ToggleLeft size={13} /> Desativar
                    </button>
                  ) : (
                    <button
                      className="w-full flex items-center gap-2 text-left px-4 py-2 text-sm hover:bg-gray-50 text-green-600"
                      onClick={() => { setMenuOpenId(null); activate.mutate(tpl.id); }}
                    >
                      <ToggleRight size={13} /> Ativar
                    </button>
                  )}
                  <div className="border-t border-gray-100 my-1" />
                  <button
                    className="w-full flex items-center gap-2 text-left px-4 py-2 text-sm hover:bg-red-50 text-red-600"
                    onClick={() => {
                      setMenuOpenId(null);
                      if (confirm('Remover template?')) deleteTemplate.mutate(tpl.id);
                    }}
                  >
                    <Trash2 size={13} /> Remover
                  </button>
                </div>
              )}
            </div>

            <p className="font-medium text-gray-900 text-sm pr-16 mb-1 truncate">{tpl.name}</p>
            {tpl.description && (
              <p className="text-xs text-gray-500 mb-2 line-clamp-2">{tpl.description}</p>
            )}

            <div className="space-y-1 text-xs text-gray-500">
              <div className="flex justify-between">
                <span>Categoria</span>
                <span className="font-medium text-gray-700">{milestoneCategoryLabels[tpl.category]}</span>
              </div>
              <div className="flex justify-between">
                <span>Métrica</span>
                <span className="font-medium text-gray-700">{metricTypeLabels[tpl.metricType]}</span>
              </div>
              <div className="flex justify-between">
                <span>Frequência</span>
                <span className="font-medium text-gray-700">{measurementFrequencyLabels[tpl.measurementFrequency]}</span>
              </div>
              <div className="flex justify-between border-t border-gray-100 pt-1 mt-1">
                <span>Aceleração</span>
                <span className="font-semibold text-indigo-600">
                  {tpl.accelerationAmount}
                  {tpl.accelerationType === 'Percentage' ? '%' :
                   tpl.accelerationType === 'Months' ? ' meses' : ' ações'}
                  {' '}({vestingAccelerationTypeLabels[tpl.accelerationType].split(' ')[0]})
                </span>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Pagination */}
      {data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 mt-4">
          <button
            disabled={!data.hasPreviousPage}
            onClick={() => setPage((p) => p - 1)}
            className="px-3 py-1 text-sm border border-gray-300 rounded-lg disabled:opacity-40 hover:bg-gray-50"
          >
            Anterior
          </button>
          <span className="text-sm text-gray-500">
            {data.pageNumber} / {data.totalPages}
          </span>
          <button
            disabled={!data.hasNextPage}
            onClick={() => setPage((p) => p + 1)}
            className="px-3 py-1 text-sm border border-gray-300 rounded-lg disabled:opacity-40 hover:bg-gray-50"
          >
            Próximo
          </button>
        </div>
      )}
    </div>
  );
}
