import { BrowserRouter, Route, Routes } from 'react-router-dom'
import './App.css'
import { EcomMarketplaceRoute } from './routes/EcomMarketplaceRoute'
import { VisualScriptRoute } from './routes/visualscript'

function App() {
  return (
    <BrowserRouter basename={import.meta.env.BASE_URL}>
      <Routes>
        <Route path="/" element={<VisualScriptRoute />} />
        <Route path="/examples/ecommarketplace" element={<EcomMarketplaceRoute />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
