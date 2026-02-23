import React from 'react';

interface VariablesListProps {
  content: string;
  className?: string;
  title?: string;
  showCount?: boolean;
}

/**
 * Componente reutilizável para extrair e exibir variáveis de templates/cláusulas
 * Extrai variáveis no formato {{variable_name}} do conteúdo
 */
const VariablesList: React.FC<VariablesListProps> = ({
  content,
  className = '',
  title = 'Variáveis Detectadas',
  showCount = true,
}) => {
  // Extrai variáveis usando regex
  const extractVariables = (text: string): string[] => {
    const regex = /\{\{([^\}]+)\}\}/g;
    const matches = text.matchAll(regex);
    const variables = Array.from(matches, (m) => m[1].trim());
    
    // Remove duplicatas e ordena alfabeticamente
    // Exclui a variável especial {{CLAUSES}} que é reservada pelo sistema
    return [...new Set(variables)]
      .filter((v) => v.toUpperCase() !== 'CLAUSES')
      .sort((a, b) => a.localeCompare(b));
  };

  const variables = extractVariables(content);

  if (variables.length === 0) {
    return (
      <div className={`text-sm text-gray-500 italic ${className}`}>
        Nenhuma variável detectada neste conteúdo.
      </div>
    );
  }

  return (
    <div className={className}>
      {title && (
        <h4 className="text-sm font-medium text-gray-700 mb-2">
          {title}
          {showCount && (
            <span className="ml-2 text-gray-500">({variables.length})</span>
          )}
        </h4>
      )}
      <div className="flex flex-wrap gap-2">
        {variables.map((variable) => (
          <span
            key={variable}
            className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 border border-blue-200"
            title={`Esta variável será solicitada durante a criação do contrato`}
          >
            <code className="font-mono">{`{{${variable}}}`}</code>
          </span>
        ))}
      </div>
      <p className="mt-2 text-xs text-gray-500">
        💡 Estas variáveis serão solicitadas no Step 4 do Contract Builder
      </p>
    </div>
  );
};

export default VariablesList;
