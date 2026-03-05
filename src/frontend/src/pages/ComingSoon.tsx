import { Construction } from 'lucide-react';

interface ComingSoonProps {
  title: string;
  description?: string;
}

export default function ComingSoon({ title, description }: ComingSoonProps) {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] px-4">
      <div className="bg-white rounded-2xl shadow-sm border border-primary-200 p-12 flex flex-col items-center gap-6 max-w-md w-full text-center">
        <div className="w-20 h-20 bg-accent/10 rounded-2xl flex items-center justify-center">
          <Construction className="w-10 h-10 text-accent" />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-primary mb-2">{title}</h1>
          <p className="text-primary-500 text-sm leading-relaxed">
            {description ?? 'Esta seção ainda está sendo desenvolvida e estará disponível em breve.'}
          </p>
        </div>
        <span className="inline-flex items-center gap-2 bg-warning/10 text-warning text-xs font-semibold px-4 py-2 rounded-full">
          <span className="w-2 h-2 rounded-full bg-warning animate-pulse" />
          Em breve
        </span>
      </div>
    </div>
  );
}
