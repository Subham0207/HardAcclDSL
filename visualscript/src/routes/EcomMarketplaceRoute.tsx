import { Link } from 'react-router-dom'

export function EcomMarketplaceRoute() {
  return (
    <div className="app-shell">
      <header className="app-header">
        <h1>Ecom Marketplace Example</h1>
        <p>Reserved route for ecommerce marketplace flow experiments.</p>
      </header>
      <div className="canvas-shell" style={{ gridTemplateColumns: '1fr' }}>
        <section className="legend">
          <h2>Route Active</h2>
          <p className="legend-help">
            This route is live at <strong>/examples/ecommarketplace</strong>. You can now add specialized UI here.
          </p>
          <Link to="/" className="legend-node-btn legend-node-btn-primary" style={{ display: 'inline-block' }}>
            Back To Visual Script
          </Link>
        </section>
      </div>
    </div>
  )
}
