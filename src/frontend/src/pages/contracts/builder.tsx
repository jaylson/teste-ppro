import { useState, useEffect } from 'react';
import { ArrowLeft, ArrowRight, Check, FileText, Loader2, Eye } from 'lucide-react';
import { Button, Card } from '@/components/ui';
import { BUILDER_STEPS } from '@/constants/contractConstants';
import { Step1SelectType } from '@/components/contracts/builder/Step1SelectType';
import { Step2AddParties } from '@/components/contracts/builder/Step2AddParties';
import { Step3SelectClauses } from '@/components/contracts/builder/Step3SelectClauses';
import { Step4FillData } from '@/components/contracts/builder/Step4FillData';
import { contractBuilderService } from '@/services/contractService';
import type { CreateContractPartyRequest } from '@/types/contract.types';

/**
 * ContractBuilderPage
 * Wizard de 5 etapas para criação de contratos
 * Rotas: /contracts/builder
 */
function ContractBuilderPage() {
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [currentStep, setCurrentStep] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Builder state
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
  const [parties, setParties] = useState<CreateContractPartyRequest[]>([]);
  const [selectedClauseIds, setSelectedClauseIds] = useState<string[]>([]);
  const [metadata, setMetadata] = useState({
    title: '',
    description: '',
    contractDate: '',
    expirationDate: '',
  });
  const [variables, setVariables] = useState<Record<string, string>>({});
  const [extractedVariables, setExtractedVariables] = useState<string[]>([]);
  const [previewHtml, setPreviewHtml] = useState<string | null>(null);

  const totalSteps = BUILDER_STEPS.length;
  const isFirstStep = currentStep === 0;
  const isLastStep = currentStep === totalSteps - 1;
  const currentStepConfig = BUILDER_STEPS[currentStep];

  // Initialize session on mount
  useEffect(() => {
    initializeSession();
  }, []);

  const initializeSession = async () => {
    try {
      setIsLoading(true);
      const session = await contractBuilderService.startBuilder();
      setSessionId(session.sessionId);
    } catch (err) {
      setError('Erro ao iniciar sessão do builder');
      console.error('Erro ao iniciar builder:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const saveStep = async () => {
    if (!sessionId) return;

    try {
      setIsSaving(true);
      setError(null);

      switch (currentStep) {
        case 0:
          // Template selection saved in state, no API call needed yet
          break;
        case 1:
          await contractBuilderService.addParties({ sessionId, parties });
          break;
        case 2:
          await contractBuilderService.selectClauses({ sessionId, clauseIds: selectedClauseIds });
          break;
        case 3:
          await contractBuilderService.fillData({
            sessionId,
            title: metadata.title,
            description: metadata.description,
            contractDate: metadata.contractDate,
            expirationDate: metadata.expirationDate,
            variables,
          });
          break;
        case 4:
          // Preview step - load preview
          await loadPreview();
          break;
      }
    } catch (err) {
      setError('Erro ao salvar etapa. Tente novamente.');
      console.error('Erro ao salvar etapa:', err);
      throw err;
    } finally {
      setIsSaving(false);
    }
  };

  const loadPreview = async () => {
    if (!sessionId) return;

    try {
      setIsLoading(true);
      const preview = await contractBuilderService.previewContract({ sessionId });
      setPreviewHtml(preview.htmlContent);
      setExtractedVariables(preview.extractedVariables);
    } catch (err) {
      setError('Erro ao carregar preview');
      console.error('Erro ao carregar preview:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const validateStep = (): boolean => {
    switch (currentStep) {
      case 0:
        // Template selection is optional
        return true;
      case 1:
        // At least one party required
        if (parties.length === 0) {
          setError('Adicione pelo menos uma parte ao contrato');
          return false;
        }
        return true;
      case 2:
        // Clauses are optional (unless template has mandatory ones)
        return true;
      case 3:
        // Title is required
        if (!metadata.title.trim()) {
          setError('O título do contrato é obrigatório');
          return false;
        }
        return true;
      case 4:
        // Preview step
        return true;
      default:
        return true;
    }
  };

  const handleNext = async () => {
    if (!validateStep()) {
      return;
    }

    try {
      await saveStep();
      if (!isLastStep) {
        setCurrentStep((prev) => prev + 1);
        setError(null);
      }
    } catch (err) {
      // Error already set in saveStep
    }
  };

  const handlePrevious = () => {
    if (!isFirstStep) {
      setCurrentStep((prev) => prev - 1);
      setError(null);
    }
  };

  const handleCancel = async () => {
    if (window.confirm('Deseja cancelar a criação do contrato? Todos os dados serão perdidos.')) {
      if (sessionId) {
        try {
          await contractBuilderService.cancelSession(sessionId);
        } catch (err) {
          console.error('Erro ao cancelar sessão:', err);
        }
      }
      window.location.href = '/contracts';
    }
  };

  const handleFinish = async () => {
    if (!sessionId) return;

    try {
      setIsLoading(true);
      setError(null);

      const contract = await contractBuilderService.generateContract({
        sessionId,
        format: 'pdf',
      });

      alert(`Contrato criado com sucesso! ID: ${contract.id}`);
      window.location.href = '/contracts';
    } catch (err) {
      setError('Erro ao gerar contrato. Tente novamente.');
      console.error('Erro ao gerar contrato:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading && !sessionId) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <Loader2 className="w-12 h-12 text-cyan-600 mx-auto mb-4 animate-spin" />
          <p className="text-gray-500">Inicializando builder...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Criar Novo Contrato</h1>
          <p className="mt-1 text-sm text-gray-500">
            Siga os passos abaixo para gerar seu contrato personalizado
          </p>
        </div>
        <Button variant="secondary" onClick={handleCancel}>
          Cancelar
        </Button>
      </div>

      {/* Progress Steps */}
      <Card className="p-6">
        <div className="flex items-center justify-between">
          {BUILDER_STEPS.map((step, index) => {
            const isActive = index === currentStep;
            const isCompleted = index < currentStep;
            const StepIcon = step.icon;

            return (
              <div key={step.id} className="flex items-center flex-1">
                {/* Step Circle */}
                <div className="flex flex-col items-center">
                  <div
                    className={`
                      w-12 h-12 rounded-full flex items-center justify-center border-2 transition-colors
                      ${
                        isCompleted
                          ? 'bg-cyan-600 border-cyan-600 text-white'
                          : isActive
                          ? 'bg-white border-cyan-600 text-cyan-600'
                          : 'bg-white border-gray-300 text-gray-400'
                      }
                    `}
                  >
                    {isCompleted ? (
                      <Check className="w-6 h-6" />
                    ) : (
                      <StepIcon className="w-6 h-6" />
                    )}
                  </div>
                  <div className="mt-2 text-center">
                    <p
                      className={`text-sm font-medium ${
                        isActive ? 'text-cyan-600' : 'text-gray-500'
                      }`}
                    >
                      {step.title}
                    </p>
                    <p className="text-xs text-gray-400">{step.description}</p>
                  </div>
                </div>

                {/* Connector Line */}
                {index < totalSteps - 1 && (
                  <div
                    className={`
                      flex-1 h-0.5 mx-4 transition-colors
                      ${isCompleted ? 'bg-cyan-600' : 'bg-gray-300'}
                    `}
                  />
                )}
              </div>
            );
          })}
        </div>
      </Card>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-sm text-red-800">
            <strong>⚠️ Erro:</strong> {error}
          </p>
        </div>
      )}

      {/* Step Content */}
      <Card className="p-8 min-h-[500px]">
        <div className="mb-6">
          <h2 className="text-2xl font-bold text-gray-900">
            {currentStepConfig.title}
          </h2>
          <p className="mt-1 text-gray-500">{currentStepConfig.description}</p>
        </div>

        {/* Render current step component */}
        {currentStep === 0 && (
          <Step1SelectType
            selectedTemplateId={selectedTemplateId}
            onSelect={setSelectedTemplateId}
          />
        )}

        {currentStep === 1 && (
          <Step2AddParties parties={parties} onUpdate={setParties} />
        )}

        {currentStep === 2 && (
          <Step3SelectClauses
            selectedClauseIds={selectedClauseIds}
            onUpdate={setSelectedClauseIds}
          />
        )}

        {currentStep === 3 && (
          <Step4FillData
            metadata={metadata}
            variables={variables}
            onUpdateMetadata={setMetadata}
            onUpdateVariables={setVariables}
            extractedVariables={extractedVariables}
          />
        )}

        {currentStep === 4 && (
          <div className="space-y-6">
            {/* Preview Loading */}
            {isLoading && (
              <div className="text-center py-12">
                <Loader2 className="w-12 h-12 text-cyan-600 mx-auto mb-4 animate-spin" />
                <p className="text-gray-500">Gerando preview...</p>
              </div>
            )}

            {/* Preview Content */}
            {!isLoading && previewHtml && (
              <>
                <div className="text-center mb-6">
                  <Eye className="w-16 h-16 text-cyan-600 mx-auto mb-4" />
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    Revise seu contrato
                  </h3>
                  <p className="text-sm text-gray-500">
                    Verifique se todas as informações estão corretas antes de gerar o documento final.
                  </p>
                </div>

                <div className="bg-white border-2 border-gray-300 rounded-lg p-8 shadow-inner">
                  <div
                    className="prose max-w-none"
                    dangerouslySetInnerHTML={{ __html: previewHtml }}
                  />
                </div>
              </>
            )}

            {/* No preview available */}
            {!isLoading && !previewHtml && (
              <div className="text-center py-12 border-2 border-dashed border-gray-300 rounded-lg">
                <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">
                  Preview indisponível
                </h3>
                <p className="text-gray-500">
                  Não foi possível gerar o preview. Clique em "Gerar Contrato" para criar o documento final.
                </p>
              </div>
            )}
          </div>
        )}
      </Card>

      {/* Navigation Buttons */}
      <div className="flex items-center justify-between">
        <Button
          variant="secondary"
          onClick={handlePrevious}
          disabled={isFirstStep || isSaving || isLoading}
          icon={<ArrowLeft className="w-4 h-4" />}
        >
          Anterior
        </Button>

        <div className="text-sm text-gray-500">
          Etapa {currentStep + 1} de {totalSteps}
        </div>

        {isLastStep ? (
          <Button
            variant="primary"
            onClick={handleFinish}
            disabled={isLoading}
            icon={
              isLoading ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <Check className="w-4 h-4" />
              )
            }
          >
            {isLoading ? 'Gerando...' : 'Gerar Contrato'}
          </Button>
        ) : (
          <Button
            variant="primary"
            onClick={handleNext}
            disabled={isSaving}
            icon={
              isSaving ? (
                <Loader2 className="w-4 h-4 animate-spin" />
              ) : (
                <ArrowRight className="w-4 h-4" />
              )
            }
          >
            {isSaving ? 'Salvando...' : 'Próximo'}
          </Button>
        )}
      </div>
    </div>
  );
}

export default ContractBuilderPage;
