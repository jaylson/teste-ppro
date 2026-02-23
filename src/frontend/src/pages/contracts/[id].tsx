import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Download, Loader2, RefreshCcw } from 'lucide-react';
import { Button, Card } from '@/components/ui';
import { ContractDetails } from '@/components/contracts/ContractDetails';
import { ContractTimeline } from '@/components/contracts/ContractTimeline';
import { contractService } from '@/services/contractService';
import type { Contract } from '@/types/contract.types';

function ContractDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [contract, setContract] = useState<Contract | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isDownloading, setIsDownloading] = useState(false);
  const [isRegenerating, setIsRegenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    loadContract(id);
  }, [id]);

  const loadContract = async (contractId: string) => {
    try {
      setIsLoading(true);
      setError(null);
      const response = await contractService.getContractById(contractId);
      setContract(response);
    } catch (err) {
      setError('Erro ao carregar detalhes do contrato.');
      console.error('Erro ao carregar contrato:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDownload = async () => {
    if (!contract) return;
    try {
      setIsDownloading(true);
      const blob = await contractService.downloadContractPdf(contract.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${contract.title}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      console.error('Erro ao baixar contrato:', err);
    } finally {
      setIsDownloading(false);
    }
  };

  const handleRegenerate = async () => {
    if (!contract) return;
    try {
      setIsRegenerating(true);
      const updated = await contractService.regenerateContract(contract.id);
      setContract(updated);
    } catch (err) {
      console.error('Erro ao regenerar contrato:', err);
    } finally {
      setIsRegenerating(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button
            variant="secondary"
            size="sm"
            icon={<ArrowLeft className="w-4 h-4" />}
            onClick={() => navigate('/contracts')}
          >
            Voltar
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Detalhes do Contrato</h1>
            <p className="text-sm text-gray-500">
              Visualize status, partes e historico do contrato
            </p>
          </div>
        </div>

        <div className="flex gap-2">
          <Button
            variant="secondary"
            size="sm"
            icon={isRegenerating ? <Loader2 className="w-4 h-4 animate-spin" /> : <RefreshCcw className="w-4 h-4" />}
            onClick={handleRegenerate}
            disabled={!contract || isRegenerating}
          >
            Regenerar PDF
          </Button>
          <Button
            variant="primary"
            size="sm"
            icon={isDownloading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
            onClick={handleDownload}
            disabled={!contract || isDownloading}
          >
            Download
          </Button>
        </div>
      </div>

      {isLoading && (
        <Card className="p-12 text-center">
          <Loader2 className="w-16 h-16 text-cyan-600 mx-auto mb-4 animate-spin" />
          <p className="text-gray-500">Carregando contrato...</p>
        </Card>
      )}

      {!isLoading && error && (
        <Card className="p-12 text-center">
          <div className="text-red-600 mb-4">⚠️</div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">Erro ao carregar</h3>
          <p className="text-gray-500 mb-4">{error}</p>
          <Button variant="primary" onClick={() => id && loadContract(id)}>
            Tentar Novamente
          </Button>
        </Card>
      )}

      {!isLoading && !error && contract && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2">
            <ContractDetails
              contract={contract}
              onContractUpdated={() => id && loadContract(id)}
            />
          </div>
          <div className="lg:col-span-1">
            <ContractTimeline contract={contract} />
          </div>
        </div>
      )}
    </div>
  );
}

export default ContractDetailsPage;
