import { BrowserRouter, Route, Routes } from 'react-router-dom'
import './App.css'
import { EcomMarketplaceRoute } from './routes/EcomMarketplaceRoute'
import { VisualScriptRoute } from './routes/visualscript'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<VisualScriptRoute />} />
        <Route path="/examples/ecommarketplace" element={<EcomMarketplaceRoute />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
