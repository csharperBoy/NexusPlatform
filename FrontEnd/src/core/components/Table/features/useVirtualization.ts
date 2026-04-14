// features/useVirtualization.ts
// import { useMemo } from 'react';
// import { VariableSizeList } from 'react-window'; // Assuming VariableSizeList for variable row heights
// import { ColumnDef, TableProps, VirtualizationOptions } from '../Table.types';

// // Props needed from the main Table component
// interface UseVirtualizationParams<T> extends Pick<TableProps<T>, 'data' | 'columns' | 'getRowHeight'> {
//   virtualizationOptions?: VirtualizationOptions;
// }

// // Return values to be used by the Table component
// interface UseVirtualizationReturn {
//   // Props to be passed to react-window's List component
//   listProps: object;
//   // A ref to the List component, if needed for programmatic scrolling
//   listRef: React.RefObject<VariableSizeList>;
//   // Potentially other helper functions or states related to virtualization
// }

// export function useVirtualization<T>({
//   data,
//   columns,
//   getRowHeight,
//   virtualizationOptions = {}, // Default options
// }: UseVirtualizationParams<T>): UseVirtualizationReturn {

//   // Destructure options with defaults
//   const {
//     height = 400, // Default height of the scrollable container
//     itemCount = data.length, // Total number of items
//     width = '100%', // Default width
//     estimatedItemSize = 50, // Estimated height for variable size lists
//     useVariableSize = !!getRowHeight, // Enable variable size if getRowHeight is provided
//     outerRef, // Ref for the outer scroll container (optional)
//     innerElementType, // Custom inner element (optional)
//     children, // Render prop for each item
//     ...restWindowProps // Any other react-window List props
//   } = virtualizationOptions;

//   const listRef = React.useRef<VariableSizeList>(null);

//   // Memoize the list props to avoid unnecessary re-renders
//   const listProps = useMemo(() => {
//     // Determine the actual item count, considering it might be controlled externally
//     const finalItemCount = typeof itemCount === 'number' ? itemCount : data.length;

//     // If virtualization is not enabled (e.g., itemCount is 0 or disabled), return empty props
//     if (!finalItemCount || !virtualizationOptions) {
//       return {};
//     }

//     // Props for react-window's List component
//     const commonProps = {
//       ref: listRef,
//       height: height,
//       width: width,
//       itemCount: finalItemCount,
//       outerRef: outerRef,
//       innerElementType: innerElementType,
//       ...restWindowProps, // Spread any other passed window props
//     };

//     if (useVariableSize && getRowHeight) {
//       // Props for VariableSizeList
//       return {
//         ...commonProps,
//         itemSize: (index: number) => getRowHeight(index) ?? estimatedItemSize, // Use provided function or estimate
//         children: ({ index, style }: { index: number; style: React.CSSProperties }) => {
//            // Render the actual row content using the provided render prop or a default
//            const RowComponent = children ?? DefaultRowRenderer;
//            const rowData = data[index]; // Access data directly
//            return <RowComponent index={index} style={style} data={{ rowData, columns }} />;
//         },
//       };
//     } else {
//       // Props for FixedSizeList (assuming default behavior if not variable)
//       // Note: You might want to explicitly use FixedSizeList if getRowHeight is not provided.
//       // For simplicity, we'll assume VariableSizeList can handle fixed sizes too,
//       // or that the user intends variable if getRowHeight is present.
//       const itemSize = typeof virtualizationOptions.itemSize === 'number' ? virtualizationOptions.itemSize : estimatedItemSize;
//       return {
//         ...commonProps,
//         itemSize: itemSize,
//          children: ({ index, style }: { index: number; style: React.CSSProperties }) => {
//            // Render the actual row content using the provided render prop or a default
//            const RowComponent = children ?? DefaultRowRenderer;
//            const rowData = data[index]; // Access data directly
//            return <RowComponent index={index} style={style} data={{ rowData, columns }} />;
//         },
//       };
//     }

//   }, [data, columns, virtualizationOptions, getRowHeight, itemCount, estimatedItemSize, children]); // Dependencies

//   // Default row renderer if no children prop is provided to virtualizationOptions
//   const DefaultRowRenderer = ({ index, style, data }: { index: number; style: React.CSSProperties; data: { rowData: T; columns: ColumnDef<T>[] } }) => {
//     const { rowData, columns } = data;
//     return (
//       <div style={style} className="tr"> {/* Basic styling, ideally replaced by Table's row component */}
//         {columns.map((column) => (
//           <div key={column.id} className="td" style={{ width: column.width || 'auto' }}>
//             {column.accessor ? column.accessor(rowData) : ''}
//           </div>
//         ))}
//       </div>
//     );
//   };


//   return {
//     listProps,
//     listRef,
//   };
// }
