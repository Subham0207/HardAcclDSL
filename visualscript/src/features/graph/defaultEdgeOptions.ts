import { MarkerType, type DefaultEdgeOptions } from '@xyflow/react'

export const defaultEdgeOptions: DefaultEdgeOptions = {
  type: 'smoothstep',
  style: { stroke: '#1f5ea0', strokeWidth: 2.2 },
  markerEnd: {
    type: MarkerType.ArrowClosed,
    color: '#1f5ea0',
  },
  labelStyle: {
    fill: '#0f3f70',
    fontWeight: 700,
    fontSize: 12,
  },
  labelBgPadding: [6, 3],
  labelBgBorderRadius: 5,
  labelBgStyle: {
    fill: '#e9f4ff',
    fillOpacity: 1,
    stroke: '#9fc1e4',
  },
}
