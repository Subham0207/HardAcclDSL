type ProductCardProps = {
  name: string
  description: string
  visual: string
  onOpen: () => void
}

export function ProductCard({ name, description, visual, onOpen }: ProductCardProps) {
  return (
    <button type="button" className="eco-product-card" onClick={onOpen}>
      <div className="eco-product-visual" aria-hidden="true">
        {visual}
      </div>
      <div className="eco-product-content">
        <h3>{name}</h3>
        <p>{description}</p>
      </div>
      <span className="eco-product-cta">View Product</span>
    </button>
  )
}
