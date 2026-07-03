export function nowIST(): string {
  return new Date().toISOString();
}

export function toISTISOString(): string {
  const date = new Date();
  const istOffset = 5.5 * 60 * 60 * 1000;
  const istDate = new Date(date.getTime() + istOffset);
  return istDate.toISOString().replace('Z', '+05:30');
}
