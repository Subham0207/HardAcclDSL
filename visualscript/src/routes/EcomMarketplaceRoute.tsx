import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ProductCard } from './ecommarketplace/ProductCard'
import marketplaceSeed from './ecommarketplaceSeed.json'
import './EcomMarketplaceRoute.css'

type ProductAttribute = {
  id: string
  name: string
  value: number
}

type ProductConfig = {
  id: string
  name: string
  visual: string
  description: string
  scriptName: string
  attributes: ProductAttribute[]
}

type CartItem = {
  id: string
  productName: string
  quantity: number
  price: number
}

type ScriptsResponse = {
  scripts: Array<{ scriptName: string }>
}

type ExecuteResponse = {
  success: boolean
  error: string
  printedLines: string[]
  returnValues: string[]
}

type MarketplaceSeed = {
  defaultUser: string
  products: ProductConfig[]
}

const seed = marketplaceSeed as MarketplaceSeed

function toIdSeed() {
  return typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
    ? crypto.randomUUID()
    : `${Date.now()}-${Math.random()}`
}

export function EcomMarketplaceRoute() {
  const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? ''
  const [products, setProducts] = useState<ProductConfig[]>(seed.products)
  const [activeProductId, setActiveProductId] = useState<string>(seed.products[0].id)
  const [mode, setMode] = useState<'live' | 'edit'>('live')
  const [userName, setUserName] = useState<string>(seed.defaultUser)
  const [availableScripts, setAvailableScripts] = useState<string[]>([])
  const [status, setStatus] = useState<string>('')
  const [calculatedPrice, setCalculatedPrice] = useState<number>(0)
  const [priceCalculated, setPriceCalculated] = useState(false)
  const [cartItems, setCartItems] = useState<CartItem[]>([])
  const [newAttributeName, setNewAttributeName] = useState('')
  const [newAttributeValue, setNewAttributeValue] = useState('0')

  const activeProduct = products.find((product) => product.id === activeProductId) ?? products[0]
  const scriptOptions = useMemo(() => Array.from(new Set(availableScripts)), [availableScripts])

  const totalPrice = useMemo(
    () => cartItems.reduce((sum, item) => sum + item.price, 0),
    [cartItems],
  )

  const setActiveProductPatch = (patch: Partial<ProductConfig>) => {
    setProducts((previous) =>
      previous.map((product) => (product.id === activeProduct.id ? { ...product, ...patch } : product)),
    )
  }

  const loadScriptsForUser = async (user: string) => {
    const endpoint = apiBaseUrl
      ? `${apiBaseUrl}/api/lua-scripts/${encodeURIComponent(user)}`
      : `/api/lua-scripts/${encodeURIComponent(user)}`

    const response = await fetch(endpoint)
    const text = await response.text()
    if (!response.ok) {
      throw new Error(`Failed loading scripts (${response.status}): ${text}`)
    }

    const payload = JSON.parse(text) as ScriptsResponse
    const names = (payload.scripts ?? []).map((x) => x.scriptName).filter(Boolean)
    setAvailableScripts(names)
  }

  const ensureUser = async () => {
    if (userName.trim()) {
      return userName.trim()
    }

    const prompted = window.prompt('Enter username to load scripts for marketplace mode:')?.trim() ?? ''
    if (!prompted) {
      throw new Error('Username is required to continue.')
    }

    setUserName(prompted)
    await loadScriptsForUser(prompted)
    return prompted
  }

  const calculatePrice = async () => {
    try {
      const user = await ensureUser()
      if (!activeProduct.scriptName.trim()) {
        setStatus('Choose a script in Edit Mode before calculating price.')
        setPriceCalculated(false)
        return
      }

      const globals: Record<string, number> = {}
      for (const attribute of activeProduct.attributes) {
        globals[attribute.name] = attribute.value
      }

      const endpoint = apiBaseUrl ? `${apiBaseUrl}/api/lua-scripts/execute` : '/api/lua-scripts/execute'
      const response = await fetch(endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          user,
          scriptName: activeProduct.scriptName,
          globals,
        }),
      })

      const text = await response.text()
      if (!response.ok) {
        setStatus(`Price calculation failed (${response.status}): ${text}`)
        setCalculatedPrice(0)
        setPriceCalculated(false)
        return
      }

      const payload = JSON.parse(text) as ExecuteResponse
      const parsed = Number(payload.printedLines?.[0] ?? '')
      const price = Number.isFinite(parsed) ? parsed : 0

      setCalculatedPrice(price)
      setPriceCalculated(true)
      setStatus(payload.success ? 'Price calculated from script output.' : payload.error || 'Script execution failed.')
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unknown error'
      setStatus(message)
      setCalculatedPrice(0)
      setPriceCalculated(false)
    }
  }

  const addToCart = () => {
    if (!priceCalculated) {
      setStatus('Calculate price first before adding to cart.')
      return
    }

    const quantity = activeProduct.attributes.find((attribute) => attribute.name === 'quantity')?.value ?? 1

    setCartItems((previous) => [
      ...previous,
      {
        id: toIdSeed(),
        productName: activeProduct.name,
        quantity,
        price: calculatedPrice,
      },
    ])
    setStatus(`${activeProduct.name} added to cart.`)
  }

  const removeAttribute = (id: string) => {
    setActiveProductPatch({
      attributes: activeProduct.attributes.filter((attribute) => attribute.id !== id),
    })
  }

  return (
    <div className="eco-page">
      <header className="eco-header">
        <h1>Ecom Marketplace Example</h1>
        <p>Simple product configurator with script-driven pricing in live mode.</p>
        <Link to="/" className="eco-top-link">
          Back To Visual Script
        </Link>
      </header>

      <main className="eco-layout">
        <section className="eco-products">
          <h2>Products</h2>
          <p className="eco-muted">Select a product card to configure attributes and calculate price.</p>
          <div className="eco-products-grid">
            {products.map((product) => (
              <ProductCard
                key={product.id}
                name={product.name}
                description={product.description}
                visual={product.visual}
                onOpen={() => {
                  setActiveProductId(product.id)
                  setCalculatedPrice(0)
                  setPriceCalculated(false)
                }}
              />
            ))}
          </div>
        </section>

        <aside className="eco-panel">
          <div>
            <h2>{activeProduct.name}</h2>
            <p className="eco-muted">{activeProduct.description}</p>
          </div>

          <div className="eco-mode-switch" role="tablist" aria-label="Product mode">
            <button type="button" data-active={mode === 'live'} onClick={() => setMode('live')}>
              Live Mode
            </button>
            <button type="button" data-active={mode === 'edit'} onClick={() => setMode('edit')}>
              Edit Mode
            </button>
          </div>

          {mode === 'edit' ? (
            <>
              <div className="eco-field">
                <label htmlFor="eco-user">User</label>
                <input
                  id="eco-user"
                  className="eco-input"
                  value={userName}
                  onChange={(event) => setUserName(event.target.value)}
                  placeholder="username"
                />
              </div>

              <button
                type="button"
                className="eco-secondary"
                onClick={async () => {
                  try {
                    const user = await ensureUser()
                    await loadScriptsForUser(user)
                    setStatus('Scripts loaded.')
                  } catch (error) {
                    setStatus(error instanceof Error ? error.message : 'Unknown error')
                  }
                }}
              >
                Load Scripts
              </button>

              <div className="eco-field">
                <label htmlFor="eco-script">Pricing Script</label>
                <select
                  id="eco-script"
                  className="eco-select"
                  value={activeProduct.scriptName}
                  onChange={(event) => setActiveProductPatch({ scriptName: event.target.value })}
                >
                  <option value="">Select script</option>
                  {scriptOptions.map((script) => (
                    <option key={script} value={script}>
                      {script}
                    </option>
                  ))}
                </select>
              </div>

              <div className="eco-field">
                <label>Attributes</label>
                <div className="eco-attrs">
                  {activeProduct.attributes.map((attribute) => (
                    <div className="eco-attr-row" key={attribute.id}>
                      <input
                        className="eco-input"
                        value={attribute.name}
                        onChange={(event) =>
                          setActiveProductPatch({
                            attributes: activeProduct.attributes.map((item) =>
                              item.id === attribute.id ? { ...item, name: event.target.value } : item,
                            ),
                          })
                        }
                      />
                      <input
                        className="eco-input"
                        type="number"
                        value={attribute.value}
                        onChange={(event) => {
                          const next = Number(event.target.value)
                          setActiveProductPatch({
                            attributes: activeProduct.attributes.map((item) =>
                              item.id === attribute.id ? { ...item, value: Number.isFinite(next) ? next : 0 } : item,
                            ),
                          })
                        }}
                      />
                      <button type="button" className="eco-danger" onClick={() => removeAttribute(attribute.id)}>
                        Remove
                      </button>
                    </div>
                  ))}
                </div>
              </div>

              <div className="eco-attr-row">
                <input
                  className="eco-input"
                  value={newAttributeName}
                  onChange={(event) => setNewAttributeName(event.target.value)}
                  placeholder="new attribute name"
                />
                <input
                  className="eco-input"
                  type="number"
                  value={newAttributeValue}
                  onChange={(event) => setNewAttributeValue(event.target.value)}
                  placeholder="default"
                />
                <button
                  type="button"
                  className="eco-secondary"
                  onClick={() => {
                    const name = newAttributeName.trim()
                    const numeric = Number(newAttributeValue)
                    if (!name || !Number.isFinite(numeric)) {
                      setStatus('Provide a valid attribute name and numeric value.')
                      return
                    }

                    setActiveProductPatch({
                      attributes: [
                        ...activeProduct.attributes,
                        { id: toIdSeed(), name, value: numeric },
                      ],
                    })
                    setNewAttributeName('')
                    setNewAttributeValue('0')
                  }}
                >
                  Add
                </button>
              </div>
            </>
          ) : (
            <>
              <div className="eco-field">
                <label>Selected Script</label>
                <span className="eco-pill">{activeProduct.scriptName || 'No script selected yet'}</span>
              </div>

              <div className="eco-field">
                <label>Attribute Values</label>
                <div className="eco-attrs">
                  {activeProduct.attributes.map((attribute) => (
                    <div className="eco-field" key={attribute.id}>
                      <label>{attribute.name}</label>
                      <input
                        className="eco-input"
                        type="number"
                        value={attribute.value}
                        onChange={(event) => {
                          const next = Number(event.target.value)
                          setActiveProductPatch({
                            attributes: activeProduct.attributes.map((item) =>
                              item.id === attribute.id ? { ...item, value: Number.isFinite(next) ? next : 0 } : item,
                            ),
                          })
                        }}
                      />
                    </div>
                  ))}
                </div>
              </div>

              <button type="button" className="eco-primary" onClick={calculatePrice}>
                Calculate
              </button>

              <div className="eco-price">
                <div className="eco-muted">Calculated Price</div>
                <strong>${calculatedPrice.toFixed(2)}</strong>
              </div>

              <button type="button" className="eco-secondary" onClick={addToCart}>
                Add To Cart
              </button>
            </>
          )}

          {status ? <p className="eco-muted">{status}</p> : null}
        </aside>

        <section className="eco-cart">
          <h2>Cart</h2>
          <div className="eco-cart-list">
            {cartItems.length === 0 ? (
              <p className="eco-muted">No products in cart yet.</p>
            ) : (
              cartItems.map((item) => (
                <article className="eco-cart-item" key={item.id}>
                  <span>
                    {item.productName} x {item.quantity}
                  </span>
                  <strong>${item.price.toFixed(2)}</strong>
                </article>
              ))
            )}
          </div>
          <div className="eco-total">Total: ${totalPrice.toFixed(2)}</div>
          <button type="button" className="eco-primary" onClick={() => setStatus('Place Order clicked (dummy).')}>
            Place Order
          </button>
        </section>
      </main>
    </div>
  )
}
