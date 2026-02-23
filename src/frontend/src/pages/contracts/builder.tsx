import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  ArrowRight,
  Check,
  Edit,
  Eye,
  FileText,
  List,
  Loader2,
  Users,
} from 'lucide-react';
import { Button, Card } from '@/components/ui';
import { BUILDER_STEPS } from '@/constants/contractConstants';
import { Step1SelectType } from '@/components/contracts/builder/Step1SelectType';
import { Step2AddParties } from '@/components/contracts/builder/Step2AddParties';
import { Step3SelectClauses } from '@/components/contracts/builder/Step3SelectClauses';
import { Step4FillData } from '@/components/contracts/builder/Step4FillData';
import { Step5PreviewGenerate } from '@/components/contracts/builder/Step5PreviewGenerate';
import { contractBuilderService } from '@/services/contractService';
import type { CreateContractPartyRequest, ContractMetadata, ContractTemplate, Clause } from '@/types/contract.types';
import { useAuthStore } from '@/stores/authStore';
import { useClientStore } from '@/stores/clientStore';

/**
 * ContractBuilderPage
 * Wizard de 5 etapas para criação de contratos
 * Rotas: /contracts/builder
 */
function ContractBuilderPage() {
  const navigate = useNavigate();
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [currentStep, setCurrentStep] = useState(0);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Builder state
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
  const [parties, setParties] = useState<CreateContractPartyRequest[]>([]);
  const [selectedClauseIds, setSelectedClauseIds] = useState<string[]>([]);
  const [metadata, setMetadata] = useState<ContractMetadata>({
    title: '',
    description: undefined,
    contractDate: undefined,
    expirationDate: undefined,
  });
  const [variables, setVariables] = useState<Record<string, string>>({});
  const [extractedVariables, setExtractedVariables] = useState<string[]>([]);

  // Full objects needed for variable extraction
  const [selectedTemplate, setSelectedTemplate] = useState<ContractTemplate | null>(null);
  const [selectedClauses, setSelectedClauses] = useState<Clause[]>([]);

  // Extract {{VARIABLE_NAME}} from template + clauses whenever they change
  useEffect(() => {
    const VARIABLE_REGEX = /\{\{([a-zA-Z0-9_]+)\}\}/g;
    const collected = new Set<string>();

    const extractFrom = (text: string) => {
      let match: RegExpExecArray | null;
      VARIABLE_REGEX.lastIndex = 0;
      while ((match = VARIABLE_REGEX.exec(text)) !== null) {
        collected.add(match[1]);
      }
    };

    if (selectedTemplate?.content) extractFrom(selectedTemplate.content);
    selectedClauses.forEach((clause) => clause.content && extractFrom(clause.content));

    setExtractedVariables(Array.from(collected));
  }, [selectedTemplate, selectedClauses]);

  const { user } = useAuthStore();
  const { selectedCompanyId } = useClientStore();
  const companyId = selectedCompanyId || user?.companyId || null;

  const totalSteps = BUILDER_STEPS.length;
  const isFirstStep = currentStep === 0;
  const isLastStep = currentStep === totalSteps - 1;
  const currentStepConfig = BUILDER_STEPS[currentStep];

  const stepIcons = {
    FileText,
    Users,
    List,
    Edit,
    Eye,
  } as const;

  const saveStep = async () => {
    if (!sessionId && currentStep !== 0) return;

    try {
      setIsSaving(true);
      setError(null);

      switch (currentStep) {
        case 0:
          if (!sessionId) {
            if (!companyId) {
              setError('Selecione uma empresa para continuar');
              throw new Error('Company required');
            }

            const session = await contractBuilderService.startBuilder({
              templateId: selectedTemplateId,
              companyId,
            });
            setSessionId(session.sessionId);
          }
          break;
        case 1:
          if (sessionId) await contractBuilderService.addParties({ sessionId, parties });
          break;
        case 2:
          if (sessionId) {
            const clauses = selectedClauseIds.map((id, index) => ({
              clauseId: id,
              displayOrder: index + 1,
              isMandatory: false,
              variables: {},
            }));
            await contractBuilderService.selectClauses({ sessionId, clauses });
          }
          break;
        case 3:
          if (sessionId) {
            await contractBuilderService.fillData({
              sessionId,
              description: metadata.description,
              contractDate: metadata.contractDate,
              expirationDate: metadata.expirationDate,
              variables,
            });
          }
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

  const validateStep = (): boolean => {
    switch (currentStep) {
      case 0:
        if (!companyId) {
          setError('Selecione uma empresa para continuar');
          return false;
        }
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
      navigate('/contracts');
    }
  };

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
      <Card className="p-4">
        <div className="grid grid-cols-5 gap-0">
          {BUILDER_STEPS.map((step, index) => {
            const isActive = index === currentStep;
            const isCompleted = index < currentStep;
            const StepIcon = stepIcons[step.icon as keyof typeof stepIcons] || FileText;

            return (
              <div key={step.id} className="flex flex-col items-center relative">
                {/* Connector line to next step */}
                {index < totalSteps - 1 && (
                  <div
                    className={`absolute top-[18px] left-1/2 w-full h-0.5 ${
                      isCompleted ? 'bg-cyan-600' : 'bg-gray-200'
                    }`}
                  />
                )}

                {/* Circle */}
                <div
                  className={`
                    relative z-10 w-9 h-9 rounded-full flex items-center justify-center border-2 transition-colors
                    ${isCompleted
                      ? 'bg-cyan-600 border-cyan-600 text-white'
                      : isActive
                      ? 'bg-white border-cyan-600 text-cyan-600'
                      : 'bg-white border-gray-200 text-gray-400'}
                  `}
                >
                  {isCompleted ? <Check className="w-4 h-4" /> : <StepIcon className="w-4 h-4" />}
                </div>

                {/* Label */}
                <p
                  className={`mt-2 text-xs font-medium text-center leading-tight px-1 ${
                    isActive ? 'text-cyan-600' : isCompleted ? 'text-gray-600' : 'text-gray-400'
                  }`}
                >
                  {step.title}
                </p>
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
            onSelectTemplate={setSelectedTemplate}
            companyId={companyId}
            onUploadSuccess={(id) => navigate(`/contracts/${id}`)}
          />
        )}

        {currentStep === 1 && (
          <Step2AddParties parties={parties} onUpdate={setParties} />
        )}

        {currentStep === 2 && (
          <Step3SelectClauses
            selectedClauseIds={selectedClauseIds}
            onUpdate={setSelectedClauseIds}
            onUpdateClauses={setSelectedClauses}
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

        {currentStep === 4 && sessionId && (
          <Step5PreviewGenerate
            sessionId={sessionId}
            onContractGenerated={(contractId) => {
              navigate(`/contracts/${contractId}`);
            }}
            onGoBack={handlePrevious}
          />
        )}
      </Card>

      {/* Navigation Buttons - Hide on last step (Step 5 has its own buttons) */}
      {!isLastStep && (
        <div className="flex items-center justify-between">
          <Button
            variant="secondary"
            onClick={handlePrevious}
            disabled={isFirstStep || isSaving}
            icon={<ArrowLeft className="w-4 h-4" />}
          >
            Anterior
          </Button>

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
        </div>
      )}
    </div>
  );
}

export default ContractBuilderPage;
