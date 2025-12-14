import "./divider.scss";

interface DividerProps {
  type?: "horizontal" | "vertical"; // optional, default horizontal
  className?: string; // optional extra classes
}

export function Divider({ type = "horizontal", className }: DividerProps) {
  return <div className={`divider divider--${type} ${className || ""}`.trim()} />;
}
