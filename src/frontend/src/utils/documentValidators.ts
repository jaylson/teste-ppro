export function normalizeDocument(value: string | undefined | null): string {
  return (value ?? '').replace(/\D/g, '');
}

export function isValidCpf(value: string): boolean {
  const cpf = normalizeDocument(value);
  if (cpf.length !== 11) return false;
  if (/^(\d)\1{10}$/.test(cpf)) return false;

  const digits = cpf.split('').map((d) => Number(d));

  const sum1 = digits
    .slice(0, 9)
    .reduce((sum, num, index) => sum + num * (10 - index), 0);
  const remainder1 = sum1 % 11;
  const firstDigit = remainder1 < 2 ? 0 : 11 - remainder1;

  const baseForSecond = [...digits.slice(0, 9), firstDigit];
  const sum2 = baseForSecond.reduce((sum, num, index) => sum + num * (11 - index), 0);
  const remainder2 = sum2 % 11;
  const secondDigit = remainder2 < 2 ? 0 : 11 - remainder2;

  return digits[9] === firstDigit && digits[10] === secondDigit;
}

export function isValidCnpj(value: string): boolean {
  const cnpj = normalizeDocument(value);
  if (cnpj.length !== 14) return false;
  if (/^(\d)\1{13}$/.test(cnpj)) return false;

  const digits = cnpj.split('').map((d) => Number(d));
  const weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  const weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

  const sum1 = weights1.reduce((sum, weight, index) => sum + digits[index] * weight, 0);
  const remainder1 = sum1 % 11;
  const digit1 = remainder1 < 2 ? 0 : 11 - remainder1;

  const sum2 = weights2.reduce((sum, weight, index) => sum + digits[index] * weight, 0);
  const remainder2 = sum2 % 11;
  const digit2 = remainder2 < 2 ? 0 : 11 - remainder2;

  return digits[12] === digit1 && digits[13] === digit2;
}
